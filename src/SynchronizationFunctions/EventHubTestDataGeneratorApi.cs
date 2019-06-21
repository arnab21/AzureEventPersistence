using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureEventPersistence.SynchronizationFunctions
{
    public static class EventHubTestDataGeneratorApi
    {
        [FunctionName("EventHubTestDataGeneratorApi")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("EventHubTestDataGeneratorApi HTTP trigger function processed a request.");

            string iterations = req.Query["iterations"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string quickStartSchema = data?.quickStartSchema ?? "SampleOrder";
            int count;

            return Int32.TryParse(iterations, out count)
                ? (ActionResult)new OkObjectResult($"Sending {iterations} messages of type {quickStartSchema}")
                : new BadRequestObjectResult("Please pass the number of messages to send on the 'iterations' query string parameter and the 'quickstartSchema' type in body");
        }
    }
}
