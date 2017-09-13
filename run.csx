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
    MyRetailProduct returnProduct = new MyRetailProduct();

    switch(req.Method.ToString())
    {
        case "GET" : 
            log.Info("Received GET command for myretail/products/{id}");
            
            string productNameJson;

            using (var client = new HttpClient())
            {
                string url = "http://redsky.target.com/v2/pdp/tcin/"+id.ToString()+"?excludes=price,taxonomy,promotion,bulk_ship,rating_and_review_reviews,rating_and_review_statistics,question_answer_statistics";
                var message = new HttpRequestMessage(HttpMethod.Get, url);

                message.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17");

                var response = await client.SendAsync(message);
                productNameJson = await response.Content.ReadAsStringAsync();
            }

            returnProduct.name = (string)JObject.Parse(productNameJson)["product"]["item"]["product_description"]["title"];
            returnProduct.id = id;
            returnProduct.current_price = new MyRetailPrice();
            returnProduct.current_price.value = productDocument.current_price.value;
            returnProduct.current_price.currency_code = productDocument.current_price.currency_code;

            returnString = JsonConvert.SerializeObject(returnProduct);
            break;
        case "PUT" :
            log.Info("Received PUT command for myretail/products/{id}");
            
            MyRetailDatabaseProduct updateProductDocument = (dynamic)productDocument;
                
            dynamic requestBodyJson = await req.Content.ReadAsAsync<object>();    
            updateProductDocument.current_price.value = (string)JObject.Parse(requestBodyJson.ToString())["current_price"]["value"];
                
            using (DocumentClient client = new DocumentClient(new Uri("https://jlt-target-case-study.documents.azure.com:443/"), "eOKMLnDZ4qGss6h7VyOunL5ZBJvNDAyOe1X1TagqTJJCJieFim4HBmiHJSuh4L87vP3Am3rO41XKvIhPfxg6pA=="))
            {
                await client.ReplaceDocumentAsync(productDocument.SelfLink, updateProductDocument);
            }

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
