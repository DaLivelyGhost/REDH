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
        public void setCMC(int index, float cmc)
        {
            Cards[index].cmc = cmc;
        }
        public void setType(int index, string type)
        {
            Cards[index].type_line = type;
        }
        public void setColorIdentity(int index, string[] identity)
        {
            Cards[index].color_identity = identity;
        }
    }
}
