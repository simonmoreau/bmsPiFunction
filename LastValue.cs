using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace bmsPiFunction
{
    public static class LastValue
    {
        [FunctionName("LastValue")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reccords/{sensorId}/lastvalue")] HttpRequestMessage req,
            [CosmosDB(
                databaseName: "bim42db",
                collectionName: "bmsPiCollection",
                ConnectionStringSetting = "myDBConnectionString",
                SqlQuery = "SELECT top 1 * FROM c where c.sensorId = {sensorId} order by c._ts desc")]IEnumerable<Reading> readings,
            ILogger log)
        {
            try
            {
                log.LogInformation("Getting the last sensor values");

                if (readings.Count() != 0)
                {
                    Reading reading = readings.FirstOrDefault();

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                                JsonConvert.SerializeObject(reading),
                                Encoding.UTF8,
                                "application/json"
                                )
                    };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject("Could not find any readings for this sensor"), Encoding.UTF8, "application/json")
                    };
                }
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message, Encoding.UTF8, "application/json") };
            }
        }

        [FunctionName("PastValues")]
        public static HttpResponseMessage Run2(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reccords/{sensorId}/pastvalues")] HttpRequestMessage req,
    [CosmosDB(
                databaseName: "bim42db",
                collectionName: "bmsPiCollection",
                ConnectionStringSetting = "myDBConnectionString",
                SqlQuery = "SELECT top 2880 * FROM c where c.sensorId = {sensorId} order by c._ts desc")]IEnumerable<Reading> readings,
    ILogger log)
        {
            try
            {
                log.LogInformation("Getting the last sensor values");

                if (readings.Count() != 0)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                                JsonConvert.SerializeObject(readings),
                                Encoding.UTF8,
                                "application/json"
                                )
                    };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject("Could not find any readings for this sensor"), Encoding.UTF8, "application/json")
                    };
                }
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message, Encoding.UTF8, "application/json") };
            }
        }

        [FunctionName("ForgeToken")]
        public static async Task<HttpResponseMessage> Run3(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "token")]HttpRequest req,
    ILogger log)
        {
            try
            {
                log.LogInformation("Getting the last sensor values");

                log.LogInformation("Geeting a Forge Token");

                string client_id = GetEnvironmentVariable("FORGE_CLIENT_ID");
                string client_secret = GetEnvironmentVariable("FORGE_CLIENT_SECRET");

                AccessToken accessToken = await GetForgeToken(client_id, client_secret);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                            JsonConvert.SerializeObject(accessToken),
                            Encoding.UTF8,
                            "application/json"
                            )
                };

            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent(ex.Message, Encoding.UTF8, "application/json") };
            }

        }

        private static async Task<AccessToken> GetForgeToken(string client_id, string client_secret)
        {
            string uri = "https://developer.api.autodesk.com/authentication/v1/authenticate";

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();

            List<KeyValuePair<string, string>> contentValues = new List<KeyValuePair<string, string>>();
            contentValues.Add(new KeyValuePair<string, string>("client_id", client_id));
            contentValues.Add(new KeyValuePair<string, string>("client_secret", client_secret));
            contentValues.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            contentValues.Add(new KeyValuePair<string, string>("scope", "data:write viewables:read data:read bucket:read"));

            FormUrlEncodedContent formContent = new FormUrlEncodedContent(contentValues);

            HttpResponseMessage response = await client.PostAsync(uri, formContent);

            string responseString = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                throw new Exception(responseString);
            }

            // Deserialize the access token from the response body.
            AccessToken accessToken = (AccessToken)JsonConvert.DeserializeObject(responseString, typeof(AccessToken));

            return accessToken;
        }

        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, System.EnvironmentVariableTarget.Process);
        }
    }
}
