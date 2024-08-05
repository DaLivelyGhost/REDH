using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rEDH
{
    [Serializable]
    public class CardList
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
