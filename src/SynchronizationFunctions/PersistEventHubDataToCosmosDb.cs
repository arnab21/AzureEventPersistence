using System;
using System.IO;
using System.Threading.Tasks;
using AzureEventPersistence.EventModels;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace AzureEventPersistence.SynchronizationFunctions
{
    public static class PersistEventHubDataToCosmosDb
    {
        [FunctionName("PersistEventHubDataToCosmosDb")]
        public static async Task Run(
            [EventHubTrigger("cachesync", Connection = "EventHubConnectionStringAppSetting")] EventData[] eventHubMessages,
            [CosmosDB(databaseName: "CosmosCache", collectionName: "CacheItems", ConnectionStringSetting = "CosmosCacheItemDbConnString")] IAsyncCollector<CosmosDbCacheItem> cacheItemsToCosmosDb,
            ILogger log)
        {
            foreach (var eventData in eventHubMessages)
            {
                var keyId = eventData.Properties[Constants.Key_Identifier];
                byte[] data = eventData.Body.AsSpan<byte>(eventData.Body.Offset, eventData.Body.Count).ToArray();

                //var message = ReadSampleMessages(eventData.Body);
                //log.LogInformation($"C# function triggered to process a message. : MessageReceivedTimeUtc: '{eventData.SystemProperties.EnqueuedTimeUtc}', OrderId: '{keyId}', OrderValue: '{message.Amount.ToString()}'");

                var cacheDocument = CosmosDbCacheItem.Build((string)keyId, Constants.Cache_TTL_InSeconds, data);
                log.LogInformation($"Saving event data to CosmosCache. : MessageReceivedTimeUtc: '{eventData.SystemProperties.EnqueuedTimeUtc}', OrderId: '{cacheDocument.Id}', cacheContent: '{cacheDocument.Content}' ");

                await cacheItemsToCosmosDb.AddAsync(cacheDocument);
            }
        }

        private static SampleOrder ReadSampleMessages(ArraySegment<byte> messageData)
        {
            SampleOrder result;
            MemoryStream ms = new MemoryStream(messageData.Array, messageData.Offset, messageData.Count);
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
