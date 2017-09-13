# myRetail REST service
Provides GET and PUT API to retrieve and set myRetail product data.
## product API
URL: https://jlt-target-case-study-myretail-proxy.azurewebsites.net/products/{id}

Parameter: id  

Used for both GET and PUT requests. The id parameter specifies which product to retrieve or update.

### GET request
Returns JSON representing product information for the given id. The product name coming from an external API and the price from an internal document database.

Response example:

```json
    {"id":50581746,"name":"Luminarc Assorted Craft Brew Glass Set - 6pc","current_price":{"value":"22.44","currency_code":"USD"}}
```
