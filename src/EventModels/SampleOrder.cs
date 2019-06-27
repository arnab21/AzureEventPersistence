using System.Collections.Generic;
using Newtonsoft.Json;

namespace AzureEventPersistence.EventModels
{
    public class SampleOrder
    {


        [JsonProperty(PropertyName = "orderId")]
        public string orderId { get; set; }

        [JsonProperty(PropertyName = "customerName")]
        public string CustomerName { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public float Amount { get; set; }

        [JsonProperty(PropertyName = "orderStatus")]
        public bool OrderCompleted { get; set; }

        [JsonProperty(PropertyName = "lineItems")]
        public Dictionary<string, float> LineItems { get; set; }

    }
}
