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
            selectedColors = [true, true, true, true, true];
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

            this.selectedColors = SelectedColors;
        }


        public string format {  get; set; }
        public string manaCurve { get; set; }
        public bool[] selectedColors {  get; set; }

    }
}
