using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

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
                log.LogInformation($"Document {count} content " + doc.GetPropertyValue<string>("content"));

                count++;
            }

        }


		private static void BuildSampleorder(string jsonContent, ILogger log)
		{


		}

	}


}
