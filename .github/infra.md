## Container App Environment

The ACA environment **must** be created with a subnet, because it's required to allow the container apps expose multiple ports. In this case, some containers, like RabbitMQ's and Seq's, will need to expose one HTTP port for the management UI and at least one TCP port, which will be used by the Kairos API to integrate with this infra.

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

# Seq

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