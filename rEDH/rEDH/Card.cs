using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace rEDH
{
    [Serializable]
    public class Card
    {

        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("mana_cost")]
        public string mana_cost { get; set; }

        [JsonPropertyName("cmc")]
        public float cmc { get; set; }

        [JsonPropertyName("image_uris")]
        public cardImages image_uris { get; set; }
    }
    public class cardImages
    {
        [JsonPropertyName("small")]
        public string small {  get; set; }
        
        [JsonPropertyName("normal")]
        public string normal { get; set; }
        
        [JsonPropertyName("large")]
        public string large {  get; set; }

        private string local;

        public void setLocalAddress(string address)
        {
            local = address;
        }
    }
}
