# Kairos

This is a monorepo for Kairos back-end services. This repo contains (or will contain) the following modules:

- **Gateway**: a mediator that redirects the incoming requests to the other modules
- **Account**: responsible for authentication, authorization, account preferences, and personal data
- **Trade**: manages the lifetime of orders
- **Portfolio**: materializes the stock positions accordingly to the settled orders
- **Banking**: responsible for money deposits and withdrawals
- **MarketData**: provides market data for other modules
- **Exchange**: act as a clearing, that matches orders in the order book

More details of the underlying architecture can be seen in [this figma file](https://www.figma.com/design/kCMWPCXieoRD1e3wMS74SC/Kairos?node-id=0-1&t=hoFPXx18zhdAWdhv-1).