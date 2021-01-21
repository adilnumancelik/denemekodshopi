using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TrialForShopi
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            string responseBody = "";
            var import_product = new ImportProductRequest();
            try
            {
                HttpResponseMessage response = await client.GetAsync("https://dev.shopiconnect.com/api/product/123");
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();

                // Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            // Convert data to ImportProductRequest model shaped json object.
            string convert = responseBody;
            string converted = "";
            JObject root2 = new JObject(); //New Json object
            JArray ProductList = new JArray();


            JObject root = JObject.Parse(convert);
            JArray variants = (JArray)root["variants"];
            JArray filters = (JArray)root["filters"];
            JObject picture = (JObject)root["picture"];
            JArray pictures = (JArray)root["pictures"];
            JArray reviews = (JArray)root["reviews"];
            JObject category = (JObject)root["category"];

            JObject color = (JObject)variants[0];
            JObject size = (JObject)variants[1];

            JArray colors = (JArray)color["features"];
            JArray sizes = (JArray)size["features"];

            for (int i = 0; i < colors.Count; i++)
            {
                for (int j = 0; j < sizes.Count; j++)
                {
                    JObject temp = new JObject();
                    JObject tempcolor = (JObject)colors[i];
                    JObject tempsize = (JObject)sizes[j];

                    temp["IntegrationId"] = tempsize["productId"];
                    temp["BaseProductCode"] = root["itemId"];

                    temp["ColorVariantCode"] = tempcolor["productId"];
                    temp["Sku"] = tempcolor["productId"];
                    temp["StockAmount"] = root["quantity"];
                    temp["Ean"] = "";
                    temp["TaxRate"] = "";
                    temp["Size"] = tempsize["displayName"];
                    temp["Title"] = root["productName"];
                    temp["Headline"] = root["headline"];
                    temp["Description"] = category["description"];
                    temp["MainCategory"] = category["name"];
                    temp["FirstSellingVat"] = "";
                    temp["LastSellingVat"] = "";
                    temp["Color"] = tempcolor["displayName"];;

                    // Pictures
                    string index;
                    int k;
                    for (k = 0; k < pictures.Count && k < 5; k++)
                    {
                        index = "Image" + (k + 1) + "Link";
                        JObject temppicture = (JObject)pictures[k];
                        temp[index] = temppicture["url"];
                    }
                    if (k < 4)
                    {
                        for (int l = k; l < 4; l++)
                        {
                            index = "Image" + (l + 1) + "Link";
                            temp[index] = "";
                        }
                    }
                    temp["WebCategory"] = root["productDetailUrl"];
                    temp["Store"] = "";
                    temp["IsDeleted"] = "";
                    temp["IsUnpublished"] = root["inStock"];
                    temp["Variant3"] = color["groupName"];
                    temp["Variant4"] = size["groupName"];

                    // Filters
                    for (k = 0; k < filters.Count && k < 4; k++)
                    {
                        index = "Filter" + (k + 3);
                        JObject tempfilter = (JObject)filters[k];
                        temp[index] = tempfilter["filterItemId"];
                    }
                    if (k < 4)
                    {
                        for (int l = k; l < 4; l++)
                        {
                            index = "Filter" + (l + 3);
                            temp[index] = "";
                        }
                    }

                    ProductList.Add(temp);
                }
            }
            root2["ProductList"] = ProductList;


            // Modified data is converted to string from Json Object.
            converted = root2.ToString();
            Console.WriteLine(converted);
            
            //Create ImportProductRequest instance. This is not mandatory but it makes sure that json fits this model.
            import_product = JsonConvert.DeserializeObject<ImportProductRequest>(converted);

            // Post the data
            try
            {
                var json = JsonConvert.SerializeObject(import_product);
                //Console.WriteLine(json);
                var data = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var url = "https://dev.shopiconnect.com/api/productImport/ImportDeltaProducts";

                var response = await client.PostAsync(url, data);

                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(response);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            
        }

    }
}
