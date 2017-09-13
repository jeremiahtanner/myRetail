# myRetail REST service
Provides GET and PUT API to retrieve and set myRetail product data.
## Product API
URL: https://jlt-target-case-study-myretail-proxy.azurewebsites.net/products/{id}

Parameter: id  

Used for both GET and PUT requests. The id parameter specifies which product to retrieve or update.

### GET request
Returns JSON representing product information for the given id. The product name coming from an external API and the price from an internal document database.

Response example:

```json
{"id":50581746,"name":"Luminarc Assorted Craft Brew Glass Set - 6pc","current_price":{"value":"22.44","currency_code":"USD"}}
```

### PUT request
Updates the price of the product for the given id with the price in the JSON in the request body.

Request body example:

```json
{"id":50581746,"current_price":{"value":"22.44","currency_code":"USD"}}
```

## Technologies
This service is hosted in Microsoft Azure. It makes use of the Function Apps serverless technology to implement the REST service. The run.csx file in this Github repository contains the C# code that the Function App serverless instance executes. This solution also makes use of Azure's Cosmos DB document database to store the price data for the products.

## How To Test
The service is publicly accessible via the URL provided above. You can use whatever HTTP request creation/execution app you would like. I used https://www.hurl.it to test.

The following are ids that can be used for testing as they are populated in the pricing database and return data from the external api call:

52568562
50581746
52310717
52412055

