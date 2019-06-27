using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AzureEventPersistence.EventModels;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace AzureEventPersistence.SynchronizationFunctions
{
    public static class CosmosCacheItemLogApi
    {
        [FunctionName("CosmosCacheItemLogApi")]
        public static void Run([CosmosDBTrigger(
            databaseName: "CosmosCache",
            collectionName: "CacheItems",
            ConnectionStringSetting = "CosmosCacheItemDbConnString",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                LogCosmosCacheMessages(input, log);
            }
        }


        private static void LogCosmosCacheMessages(IReadOnlyList<Document> documents, ILogger log)
        {
            int count = 0;
            foreach (var doc in documents)
            {
                log.LogInformation($"Document {count} " + doc.ToString());
                var contentBytes = Convert.FromBase64String(doc.GetPropertyValue<string>("content"));
                var sampleOrder = DeserializeContentToSampleorder(contentBytes);
                var lineItems = string.Join(", ", sampleOrder.LineItems.Select(kv => kv.Key + " = " + kv.Value).ToArray());
                log.LogInformation($"OrderId: {sampleOrder.orderId}, Amount: {sampleOrder.Amount}, Name: {sampleOrder.CustomerName}, LineItems: {lineItems}");

                count++;
            }

        }


		private static SampleOrder DeserializeContentToSampleorder(byte[] contentBytes)
		{
            SampleOrder result;
            MemoryStream ms = new MemoryStream(contentBytes);
            using (BsonReader reader = new BsonReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                result = serializer.Deserialize<SampleOrder>(reader);
            }
            ms.Dispose();
            return result;
        }

	}


}
