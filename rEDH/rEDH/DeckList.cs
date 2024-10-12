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
        private Card[] Cards;

        public DeckList() 
        {
            Cards = new Card[100];
        
        }
        public Card[] getDeck()
        {
            return Cards;
        }
        public void setCommander(Card commander)
        {
            Cards[0] = commander;
        }
        //public void addCard(Card toAdd)
        //{
        //    if(Cards == null)
        //    {
        //        Cards = new List<Card>();
        //    }

        //    Cards.Add(toAdd);
        //}
    }
}
