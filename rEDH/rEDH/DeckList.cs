using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rEDH
{
    /// <summary>
    /// Object full of card objects which represents the decklist. 
    /// </summary>
    [Serializable]
    public class DeckList
    {
        public List<Card> Cards { get; set; }


        public void addCard(Card toAdd)
        {
            if(Cards == null)
            {
                Cards = new List<Card>();
            }

            Cards.Add(toAdd);
        }
    }
}
