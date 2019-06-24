using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using AzureEventPersistence.EventModels;



namespace AzureEventPersistence.SynchronizationFunctions
{
    public static class EventHubTestDataGeneratorApi
    {
        [FunctionName("EventHubTestDataGeneratorApi")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [EventHub("cachesync", Connection = "EventHubConnectionStringAppSetting")] IAsyncCollector<EventData> outputEvents,
            ILogger log) 
        {
            log.LogInformation("EventHubTestDataGeneratorApi HTTP trigger function processed a request.");

            string iterations = req.Query["iterations"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string quickStartSchema = data?.quickStartSchema ?? "SampleOrder";
            int count;

            if (!Int32.TryParse(iterations, out count))                
                return new BadRequestObjectResult("Please pass the number of messages to send on the 'iterations' query string parameter and the 'quickstartSchema' type in body");

            try
            {
                // Creates a collection of 'count' EventData messages to be sent to output event hub.
                for (var i = 0; i < count; i++)
                {
                    var messageTuple = GenerateSampleMessages(i, quickStartSchema);                    
                    var sampleEventData = new EventData(messageTuple.Item2);
                    sampleEventData.Properties[Constants.Key_Identifier] = messageTuple.Item1;
                    log.LogTrace($"Building message: {i}, OrderId: {messageTuple.Item1}");


                    //See: https://github.com/Azure/azure-webjobs-sdk/issues/1643. No way yet to use the PartitionKey binding via Azure FnApps
                    await outputEvents.AddAsync(sampleEventData);
                }                
            }
            catch (Exception exception)
            {
                log.LogError($"{DateTime.Now} > Exception: {exception.Message}");
            }
            
            return (ActionResult)new OkObjectResult($"Sent {iterations} messages of type {quickStartSchema} to EventHub");
        }


        private static Tuple<string, byte[]> GenerateSampleMessages(int counter, string quickStartMessageType)
        {
            switch (quickStartMessageType)
            {
                case "SampleOrder":
                    return GenerateSampleOrderMessages(counter);
                default:
                    return GenerateSampleOrderMessages(counter);
            }

        }

        private static Tuple<string, byte[]> GenerateSampleOrderMessages(int counter)
        {
            SampleOrder cacheObj = new SampleOrder
            {
                orderId = $"M1234_{counter}",
                Amount = counter * 12.21F,
                CustomerName = $"Bruce Atwood_{counter}",
                OrderCompleted = false,
                LineItems = new Dictionary<string, float>()
            };
            //Add order items
            cacheObj.LineItems.Add("Banana", counter * 2.00F);
            cacheObj.LineItems.Add("Juice", counter * 1.50F);
            cacheObj.LineItems.Add("Carrots", counter * 6.20F);


            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, cacheObj);
            }

            return new Tuple<string, byte[]>(cacheObj.orderId, ms.ToArray());
        }



    }
}
