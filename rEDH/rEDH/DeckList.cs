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
            
            for(int i = 0; i < Cards.Length; i++)
            {
                Cards[i] = new Card();
            }

        }
        public Card[] getDeck()
        {
            return Cards;
        }
        public Card getCard(int index)
        {
            return Cards[index];
        }
        public void setCard(int index, Card toSet)
        {
            Cards[index] = toSet;
        }
        public void setCommander(Card commander)
        {
            Cards[0] = commander;
        }
        //-----------Individual Card Properties------------------------
        //-----------Blueprint functions
        public void setCMC(int index, float cmc)
        {
            Cards[index].cmc = cmc;
        }
        public void setTypeLine(int index, string type)
        {
            Cards[index].type_line = type;
        }
        public void setColorIdentity(int index, string[] identity)
        {
            Cards[index].color_identity = identity;
        }
        public string getTypeLine(int index)
        {
            return Cards[index].type_line;
        }
        //Sorting algorithms------------------------------------------
        public void nameSort()
        {
            //starting at 1 to ignore the commander. Keeping that at position 0 always.
            for(int i = 1; i < Cards.Length - 1; i++)
            {
                for(int j = i + 1; j < Cards.Length; j++)
                {
                    if (String.Compare(Cards[i].name, Cards[j].name) == 1)
                    {
                        Card temp = Cards[i];
                        Cards[i] = Cards[j];
                        Cards[j] = temp;    
                    }
                }
            }
        }
        public void cmcSort()
        {
            //starting at 1 to ignore the commander. Keeping that at position 0 always.
            for (int i = 1; i < Cards.Length - 1; i++)
            {
                for(int j = i + 1; j < Cards.Length; j++)
                {
                    if (Cards[i].cmc > Cards[j].cmc)
                    {
                        Card temp = Cards[i];
                        Cards[i] = Cards[j];
                        Cards[j] = temp;

                    }
                    else if(String.Compare(Cards[i].name, Cards[j].name) == 1 && Cards[i].cmc == Cards[j].cmc)
                    {
                        Card temp = Cards[i];
                        Cards[i] = Cards[j];
                        Cards[j] = temp;
                    }
                }

            }
        }
        public void typeSort()
        {
            //starting at 1 to ignore the commander. Keeping that at position 0 always.
            for (int i = 1; i < Cards.Length - 1; i++)
            {
                for (int j = i + 1; j < Cards.Length; j++)
                {
                    if (String.Compare(Cards[i].card_type[0], Cards[j].card_type[0]) == 1)
                    {
                        Card temp = Cards[i];
                        Cards[i] = Cards[j];
                        Cards[j] = temp;
                    }
                }
            }
        }
    }
}
