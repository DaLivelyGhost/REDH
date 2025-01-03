using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace rEDH
{
    public class DeckDefinitions
    {
        public DeckDefinitions() {
            format = "EDH";
            manaCurve = "Low Range";
            selectedColors = ["W", "U", "B", "R", "G"];
        }
        public DeckDefinitions(bool[] SelectedColors, string Format, string ManaCurve)
        {

            if(Format != null)
            {
                format = Format;
            }
            else
            {
                format = "EDH";
            }
            if(ManaCurve != null)
            {
                this.manaCurve = ManaCurve;
            }
            else
            {
                manaCurve = "Low Range";
            }

            setSelectedColors(SelectedColors);
        }


        public string format {  get; set; }
        public string manaCurve { get; set; }
        public string[] selectedColors {  get; set; }

        private void setSelectedColors(bool[] colors)
        {
            List<string> deckColors = new List<string>();
            
            if (colors[0])
            {
                deckColors.Add("W");
            }
            if (colors[1])
            {
                deckColors.Add("U");
            }
            if (colors[2])
            {
                deckColors.Add("B");
            }
            if (colors[3])
            {
                deckColors.Add("R");
            }
            if (colors[4])
            {
                deckColors.Add("G");
            }

            selectedColors = deckColors.ToArray();
        }
    }
}
