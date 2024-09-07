using System;
using System.Collections.Generic;
using System.IO;
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

        private string typeLine;
        [JsonPropertyName("type_line")]
        public string type_line {
            get
            {
                return typeLine;
            }
            set
            {

                typeLine = value;

                if (typeLine.Contains("Legendary"))
                {
                    isLegendary = true;
                    
                }
                else
                {
                    isLegendary = false;
                }
                card_type = new List<String>();

                if(typeLine.Contains("Creature"))
                {
                    card_type.Add("Creature");
                }
                if(typeLine.Contains("Artifact"))
                {
                    card_type.Add("Artifact");
                }
                if (typeLine.Contains("Enchantment"))
                {
                    card_type.Add("Enchantment");
                }
                if (typeLine.Contains("Sorcery"))
                {
                    card_type.Add("Sorcery");
                }
                if (typeLine.Contains("Instant"))
                {
                    card_type.Add("Instant");
                }
                if (typeLine.Contains("Land"))
                {
                    card_type.Add("Land");
                }
                if (typeLine.Contains("Planeswalker"))
                {
                    card_type.Add("Planeswalker");
                }
            }
        }
        [JsonIgnore]
        public List<string> card_type { get; set; }

        private bool legendary;
        [JsonIgnore]
        public bool isLegendary {
            get => legendary;
            set
            {
                legendary = value;
            }
        }




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

    }
}
