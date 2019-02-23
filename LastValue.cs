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
    }
}
