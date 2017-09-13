#r "Newtonsoft.Json"
#r "Microsoft.Azure.Documents.Client"

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, dynamic productDocument, int? id, TraceWriter log)
{
    string returnString = String.Empty;

    //Object used to represent the product JSON for serialization purposes.
    MyRetailProduct returnProduct = new MyRetailProduct();

    //Determine if the request is a GET or PUT
    switch(req.Method.ToString())
    {
        case "GET" : 
            log.Info("Received GET command for myretail/products/{id}");
            
            string productNameJson;

            //Make a GET request to the redsky.target.com/v2/pdp/tcin REST api to get the product name.
            using (var client = new HttpClient())
            {
                string url = "http://redsky.target.com/v2/pdp/tcin/"+id.ToString()+"?excludes=price,taxonomy,promotion,bulk_ship,rating_and_review_reviews,rating_and_review_statistics,question_answer_statistics";
                var message = new HttpRequestMessage(HttpMethod.Get, url);

                message.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17");

                var response = await client.SendAsync(message);
                productNameJson = await response.Content.ReadAsStringAsync();
            }

            //Parse the JSON returned from the GET request to get the product name. 
            returnProduct.name = (string)JObject.Parse(productNameJson)["product"]["item"]["product_description"]["title"];
            
            returnProduct.id = id;
            
            //Set the price using the productDocument object populated from the input binding to the Cosmos DocumentDB.
            returnProduct.current_price = new MyRetailPrice();
            returnProduct.current_price.value = productDocument.current_price.value;
            returnProduct.current_price.currency_code = productDocument.current_price.currency_code;

            //Serialize the product object so that it can be returned as a JSON string in the response.
            returnString = JsonConvert.SerializeObject(returnProduct);
            
            break;
        case "PUT" :
            log.Info("Received PUT command for myretail/products/{id}");
            
            //Object used to update the price for the document linked to by the productDocument object provided by the input binding.
            MyRetailDatabaseProduct updateProductDocument = (dynamic)productDocument;
            
            //Get the JSON from the request body, parse it, and set the price to the new value.
            dynamic requestBodyJson = await req.Content.ReadAsAsync<object>();    
            updateProductDocument.current_price.value = (string)JObject.Parse(requestBodyJson.ToString())["current_price"]["value"];

            //Connect to the Cosmos DocumentDB that contains the product collection of documents. Replace the linked document with the updated one.   
            using (DocumentClient client = new DocumentClient(new Uri("https://jlt-target-case-study.documents.azure.com:443/"), "eOKMLnDZ4qGss6h7VyOunL5ZBJvNDAyOe1X1TagqTJJCJieFim4HBmiHJSuh4L87vP3Am3rO41XKvIhPfxg6pA=="))
            {
                await client.ReplaceDocumentAsync(productDocument.SelfLink, updateProductDocument);
            }

            //Serialize the updated JSON to get returned in the response.
            returnString = JsonConvert.SerializeObject(updateProductDocument);

            break;
    }
    
    return id == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please specify the id you want to search for.")
        : req.CreateResponse(HttpStatusCode.OK, returnString );
}

public class MyRetailProduct
{
    public int? id;
    public string name;
    public MyRetailPrice current_price;
}

public class MyRetailDatabaseProduct
{
    public string id;
    public MyRetailPrice current_price;
}

public class MyRetailPrice
{
    public string value; 
    public string currency_code;
}
