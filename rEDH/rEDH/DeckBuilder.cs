using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rEDH
{
    internal class DeckBuilder
    {
        DeckList deckList;
        string colorIdentitySearch;
        public DeckBuilder() 
        {
            deckList = new DeckList();
        }
        public Card[] buildDeck(DatabaseWrangler dbWrangler, bool white, bool blue, bool black, bool red, bool green)
        {
            setColorIdentitySearchTerms(white, blue, black, red, green);

            //sets the first spot in the card array to the commander
            deckList.setCommander(dbWrangler.queryCommander(colorIdentitySearch));

            return deckList.getDeck();
        }
        private void setColorIdentitySearchTerms(bool white, bool blue, bool black, bool red, bool green)
        {
            bool[] colors = { white, blue, black, red, green };
           

            //search tokens will be different depending on if the deck is monocolored or not.
            int colorCount = 0;
            foreach(bool color in colors)
            {
                if (color) { colorCount++; }
            }

            //monocolored
            if (colorCount == 1)
            {
                colorIdentitySearch = "colorIdentity LIKE ";

                for (int i = 0; i < colors.Length; i++)
                {
                    if (colors[i])
                    {
                        switch (i)
                        {
                            case 0:
                                colorIdentitySearch += "'W'";
                                break;
                            case 1:
                                colorIdentitySearch += "'U'";
                                break;
                            case 2:
                                colorIdentitySearch += "'B'";
                                break;
                            case 3:
                                colorIdentitySearch += "'R'";
                                break;
                            case 4:
                                colorIdentitySearch += "'G'";
                                break;
                        }

                    }
                }
            }
            //colorless
            else if (colorCount == 0)
            {
                colorIdentitySearch = "colorIdentity LIKE ''";
            }
            //multicolored
            else
            {
                bool first = true;
                colorIdentitySearch = "";

                for (int i = 0; i < colors.Length; i++)
                {
                    if (!first)
                    {
                        colorIdentitySearch += " AND ";
                    }
                    if (!colors[i])
                    {
                        colorIdentitySearch += "colorIdentity NOT LIKE ";

                        switch (i)
                        {
                            case 0:
                                colorIdentitySearch += "'%W%'";
                                break;
                            case 1:
                                colorIdentitySearch += "'%U%'";
                                break;
                            case 2:
                                colorIdentitySearch += "'%B%'";
                                break;
                            case 3:
                                colorIdentitySearch += "'%R%'";
                                break;
                            case 4:
                                colorIdentitySearch += "'%G%'";
                                break;
                        }
                    }
                    else
                    {
                        colorIdentitySearch += "colorIdentity LIKE ";

                        switch (i)
                        {
                            case 0:
                                colorIdentitySearch += "'%W%'";
                                break;
                            case 1:
                                colorIdentitySearch += "'%U%'";
                                break;
                            case 2:
                                colorIdentitySearch += "'%B%'";
                                break;
                            case 3:
                                colorIdentitySearch += "'%R%'";
                                break;
                            case 4:
                                colorIdentitySearch += "'%G%'";
                                break;
                        }
                    }

                    first = false;
                }
            }
        }
    }
}
