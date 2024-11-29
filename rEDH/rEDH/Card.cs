using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

///<summary>
///Object representing card objects.
///Image URIs are images representing the card's art. Not stored locally, and are streamed from scryfall.
/// </summary>
namespace rEDH
{
    [Serializable]
    public class Card
    {
        public Card()
        {
            name = "";
            mana_cost = "";
            color_identity = [""];
            type_line = "";
            card_type = new List<string>();
            image_uris = new cardImages();
            legalities = new formatLegalities();
        }


        [JsonPropertyName("name")]
        public string name { get; set; }

        private string manaCost;
        [JsonPropertyName("mana_cost")]
        public string mana_cost {
            get => manaCost; 
            set
            {
                manaCost = value;
            }
        }
        //----------------------------------------------------------------
        //Color Identity
        //----------------------------------------------------------------
        [JsonIgnore]
        private string[] colorIdentity { get; set; }
        [JsonPropertyName("color_identity")]
        public string[] color_identity {
            get => colorIdentity; 
            set
            {
                colorIdentity = value;
                string arrayString = "";

                foreach(string s in colorIdentity)
                {
                    arrayString += s;
                }
                color_identity_string = arrayString;
            } 
        
        }
        [JsonIgnore]
        public string color_identity_string { get; set; }
        //----------------------------------------------------------------
        [JsonPropertyName("cmc")]
        public float cmc { get; set; }

        //----------------------------------------------------------------
        //Card Type
        //----------------------------------------------------------------
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
        //----------------------------------------------------------------

        private bool legendary;
        [JsonIgnore]
        public bool isLegendary {
            get => legendary;
            set
            {
                legendary = value;
            }
        }
        [JsonPropertyName("layout")]
        public string layout { get; set; }

        //----------------------------------------------------------------
        //Card Images
        //----------------------------------------------------------------
        [JsonPropertyName("image_uris")]
        public cardImages image_uris { get; set; }
        [JsonPropertyName("legalities")]
        public formatLegalities legalities { get; set; } 
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
    public class formatLegalities
    {
        [JsonPropertyName("commander")]
        public string commander { get; set; }

        [JsonPropertyName("paupercommander")]
        public string paupercommander {  get; set; }

        [JsonPropertyName("predh")]
        public string predh {  get; set; }
    }
}
