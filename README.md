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

# How to run it locally

To run the app outside of a docker container, log in to Azure, in order to get access to the Azure KV secrets in a passwordless manner:

```
az login
```

If you need to run the app inside of a container, through `docker-compose.yml` for example, it's required to define the Service Principal credentials in a `.env` file:
```
AZURE_CLIENT_ID=
AZURE_CLIENT_SECRET=
```