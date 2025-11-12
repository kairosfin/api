# Kairos

This is a monorepo for Kairos back-end services. This repo contains (or will contain) the following modules:

- **Gateway**: a mediator that redirects the incoming requests to the other modules
- **Account**: responsible for authentication, authorization, account preferences and personal data
- **Trade**: manages the lifetime of orders
- **Portfolio**: materializes the stock positions accordingly to the settled orders
- **Banking**: responsible for money deposits and withdrawals
- **MarketData**: provides market data for other modules
- **Exchange**: act as a clearing that matches orders in the order book

More details on the underlying architecture can be seen in [this figma file](https://www.figma.com/design/kCMWPCXieoRD1e3wMS74SC/Kairos?node-id=0-1&t=hoFPXx18zhdAWdhv-1).

<img width="1476" height="1286" alt="High Level Architecture" src="https://github.com/user-attachments/assets/c34f642b-bd73-49c9-bd2c-5ebde48eb143" />

#  Azure resources

- Kairos Broker app: https://capp-kairos-broker.yellowriver-1d32555a.eastus2.azurecontainerapps.io/docs
- Kairos RabbitMQ: https://capp-kairos-rabbitmq.yellowriver-1d32555a.eastus2.azurecontainerapps.io/
- Kairos Seq: https://capp-kairos-seq.yellowriver-1d32555a.eastus2.azurecontainerapps.io/

# How to run it locally

To run the app outside of a docker container, log in to Azure, in order to get access to the Azure KV secrets in a passwordless manner:

```sh
az login
```

Then run only the required infra as containers, while the app is ran normally via kestrel server:
```sh
docker --env-file ./.env compose up --build -d seq.kairos rabbitmq.kairos

cd src/Gateway && dotnet run --lp Local
```

If you need to run the app inside of a container, through `docker-compose.yml` for example, it's required to define the Service Principal credentials in a `.env` file, instead of simply doing `az login`, as mentioned earlier:
```
AZURE_CLIENT_ID=
AZURE_CLIENT_SECRET=
```

After setting the correct values you can simply execute `docker compose up -d`

# Infra

After manually creating some basic resources such as RG, KV and ACR, the following instructions might come in handy for automatically creating more complex infra, such as vnet, subnet, storage account, file share, ACA environment, the ACAs themselves, etc.

## Container App Env

The Azure Container App (ACA) Environment **must** be created with a subnet, because it's required to allow the container apps expose multiple ports. In this case, some containers, like RabbitMQ's and Seq's, will need to expose one HTTP port for the management UI and at least one TCP port, which will be used by the Kairos API to integrate with this infra.

```bash
RG_NAME="kairos"
LOCATION="eastus2"
VNET_NAME="vnet-kairos"
SUBNET_NAME="cae-subnet"

az network vnet create \
  --resource-group $RG_NAME \
  --name $VNET_NAME \
  --location $LOCATION \
  --address-prefix 10.0.0.0/16

az network vnet subnet create \
  --resource-group $RG_NAME \
  --vnet-name $VNET_NAME \
  --name $SUBNET_NAME \
  --address-prefix 10.0.1.0/27 \
  --delegations "Microsoft.App/environments"

SUBNET_ID=$(az network vnet subnet show --resource-group $RG_NAME --vnet-name $VNET_NAME --name $SUBNET_NAME --query id -o tsv)

az containerapp env create \
  --name cae-kairos \
  --resource-group $RG_NAME \
  --location $LOCATION \
  --infrastructure-subnet-resource-id $SUBNET_ID
```

## RabbitMQ

Before creating the RabbitMQ container app, it's required to create an Azure File Share that'll be used for mounting the volume, in order to persist the Rabbit data.

```sh
STORAGE_ACCOUNT="kairostoraging"

# Create a Storage Account
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group kairos \
  --location eastus2 \
  --sku Standard_LRS

# Get the Storage Account Key
STORAGE_KEY=$(az storage account keys list -g kairos -n $STORAGE_ACCOUNT --query "[0].value" -o tsv)

# Create the File Share
FILE_SHARE="fs-kairos-rabbitmq"

az storage share create \
  --name $FILE_SHARE \
  --account-name $STORAGE_ACCOUNT \
  --account-key $STORAGE_KEY

# Link ACA env to the storage
az containerapp env storage set \
  --name cae-kairos \
  --resource-group kairos \
  --storage-name rabbitmq-data \
  --azure-file-account-name $STORAGE_ACCOUNT \
  --azure-file-account-key $STORAGE_KEY \
  --azure-file-share-name $FILE_SHARE \
  --access-mode ReadWrite
```

Now the ACA can be created:

```sh
az containerapp create \
  --name capp-kairos-rabbitmq \
  --resource-group kairos \
  --environment cae-kairos \
  --yaml .github/capp-kairos-rabbitmq.yml
```

## Seq

Assuming that the storage account was already created because of RabbitMQ, then the seq infra creation would be the following:

```sh
STORAGE_ACCOUNT="kairostoraging"

# Get the Storage Account Key
STORAGE_KEY=$(az storage account keys list -g kairos -n $STORAGE_ACCOUNT --query "[0].value" -o tsv)

# Create the File Share
FILE_SHARE="fs-kairos-seq"

az storage share create \
  --name $FILE_SHARE \
  --account-name $STORAGE_ACCOUNT \
  --account-key $STORAGE_KEY

# Link ACA env to the storage
az containerapp env storage set \
  --name cae-kairos \
  --resource-group kairos \
  --storage-name seq-data \
  --azure-file-account-name $STORAGE_ACCOUNT \
  --azure-file-account-key $STORAGE_KEY \
  --azure-file-share-name $FILE_SHARE \
  --access-mode ReadWrite
```

Now the ACA can be created:

```sh
az containerapp create \
  --name capp-kairos-seq \
  --resource-group kairos \
  --environment cae-kairos \
  --yaml .github/capp-kairos-seq.yml
```

## Kairos Broker

Before creating the app itself, a user assigned managed identity should be created, because it'll be granted a ACR Pull permission, which will be used by the capp-kairos-broker creation YAML.

```sh
az identity create -n kairos-service -g kairos

PRINCIPAL_ID=$(az identity show \
  --name kairos-service \
  --resource-group kairos \
  --query principalId \
  --output tsv)

az keyvault set-policy \
  --name kv-kairos \
  --resource-group kairos \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list

ACR_ID=$(az acr show \
  --name kairosfinance \
  --resource-group kairos \
  --query id \
  --output tsv)

az role assignment create \
  --assignee $PRINCIPAL_ID \
  --scope $ACR_ID \
  --role "AcrPull"
```

Now the app can be created:

```sh
docker build --no-cache \
  -t kairosfinance.azurecr.io/kairos/broker . \
  -f ./src/Gateway/Dockerfile

docker push kairosfinance.azurecr.io/kairos/broker

az containerapp create \
  --name capp-kairos-broker \
  --resource-group kairos \
  --environment cae-kairos \
  --yaml .github/capp-kairos-broker.yml
```