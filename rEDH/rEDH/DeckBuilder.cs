using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;

namespace rEDH
{
    internal class DeckBuilder
    {
        DeckList deckList;
        //The integers represent cmc. Example: each 1 is a card that costs 1 mana.
        static int[] lowrangeCurve = {1,1,1,1,1,1,1,1,1,2,
                                      2,2,2,2,2,2,2,2,2,2,
                                      2,2,2,2,2,2,2,2,2,2,
                                      2,2,2,2,3,3,3,3,3,3,
                                      3,3,3,3,3,3,3,3,3,3,
                                      3,3,3,3,3,3,4,4,4,4,
                                      4,4,4,5,5};
        static int lowrangeLand = 34; //34 instead of 35 cuz we need room for commander

        static int[] midrangeCurve = {};
        static int midrangeLand;

        static int[] hirangeCurve = { };
        static int hirangeLand;

        //commander has their own color identity search because they absolutely need to include all colors selected by user
        string commanderColorIdentitySearch;
        string colorIdentitySearch;
        public DeckBuilder() 
        {
            deckList = new DeckList();
        }
        public Card[] buildDeck(DatabaseWrangler dbWrangler, bool white, bool blue, bool black, bool red, bool green)
        {
            //set search terms to find cards in the colors we want. Example: colorIdentity LIKE 'G';
            setColorIdentitySearchTerms(white, blue, black, red, green);

            //sets the first spot in the card array to the commander

            deckList.setCommander(dbWrangler.queryCommander(commanderColorIdentitySearch));

            //Blueprint out the cards' cmc, card types, and colors. This gives us a general framework of what we want from the db
            //when we query for random cards
            blueprintCards(white, blue, black, red, green);

            //query the db for card name, effects and card arts based on the blueprint we've established.
            queryCards(dbWrangler);

            //return to display.
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
                commanderColorIdentitySearch = "colorIdentity LIKE ";
                colorIdentitySearch = "colorIdentity LIKE ";

                for (int i = 0; i < colors.Length; i++)
                {
                    if (colors[i])
                    {
                        switch (i)
                        {
                            case 0:
                                commanderColorIdentitySearch += "'W'";
                                colorIdentitySearch += "'W'";
                                break;
                            case 1:
                                commanderColorIdentitySearch += "'U'";
                                colorIdentitySearch += "'U'";
                                break;
                            case 2:
                                commanderColorIdentitySearch += "'B'";
                                colorIdentitySearch += "'B'";
                                break;
                            case 3:
                                commanderColorIdentitySearch += "'R'";
                                colorIdentitySearch += "'R'";
                                break;
                            case 4:
                                commanderColorIdentitySearch += "'G'";
                                colorIdentitySearch += "'R'";
                                break;
                        }

                    }
                }
            }
            //colorless
            else if (colorCount == 0)
            {
                commanderColorIdentitySearch = "colorIdentity LIKE ''";
                colorIdentitySearch = "colorIdentity LIKE ''";
            }
            //multicolored
            else
            {
                bool first = true;
                commanderColorIdentitySearch = "";
                colorIdentitySearch = "";

                for (int i = 0; i < colors.Length; i++)
                {
                    if (!colors[i])
                    {
                        if (!first)
                        {
                            commanderColorIdentitySearch += " AND ";
                            colorIdentitySearch += " AND ";
                        }
                        commanderColorIdentitySearch += "colorIdentity NOT LIKE ";
                        colorIdentitySearch += "colorIdentity NOT LIKE ";

                        switch (i)
                        {
                            case 0:
                                commanderColorIdentitySearch += "'%W%'";
                                colorIdentitySearch += "'%W%'";
                                break;
                            case 1:
                                commanderColorIdentitySearch += "'%U%'";
                                colorIdentitySearch += "'%U%'";
                                break;
                            case 2:
                                commanderColorIdentitySearch += "'%B%'";
                                colorIdentitySearch += "'%B%'";
                                break;
                            case 3:
                                commanderColorIdentitySearch += "'%R%'";
                                colorIdentitySearch += "'%R%'";
                                break;
                            case 4:
                                commanderColorIdentitySearch += "'%G%'";
                                colorIdentitySearch += "'%G%'";
                                break;
                        }
                    }
                    else
                    {
                        if (!first)
                        {
                            commanderColorIdentitySearch += " AND ";
                        }
                        commanderColorIdentitySearch += " colorIdentity LIKE ";

                        switch (i)
                        {
                            case 0:
                                commanderColorIdentitySearch += "'%W%'";
                                break;
                            case 1:
                                commanderColorIdentitySearch += "'%U%'";
                                break;
                            case 2:
                                commanderColorIdentitySearch += "'%B%'";
                                break;
                            case 3:
                                commanderColorIdentitySearch += "'%R%'";
                                break;
                            case 4:
                                commanderColorIdentitySearch += "'%G%'";
                                break;
                        }
                    }

                    first = false;
                }
            }
        }

        //sets the card types and colors of each card in the deck following the curve.
        private void blueprintCards(bool white, bool blue, bool black, bool red, bool green)
        {

            //starting at 1 because 0 is the commander and we have that already.
            for(int i = 1; i < 100; i++)
            {
                Random rndm = new Random();
                int randomType;

                //We'll iterate the non-lands first, and then the lands.
                //i - 1 for curve array because i starts at 1.
                if(i - 1 < lowrangeCurve.Length)
                {
                    //set total cmc
                    deckList.setCMC(i, lowrangeCurve[i-1]);

                    //set type
                    randomType = rndm.Next(0, 5);

                    switch (randomType)
                    {
                        case 0:
                            deckList.setType(i, "Artifact");
                            break;
                        case 1:
                            deckList.setType(i, "Creature");
                            break;
                        case 2:
                            deckList.setType(i, "Enchantment");
                            break;
                        case 3:
                            deckList.setType(i, "Planeswalker");
                            break;
                        case 4:
                            deckList.setType(i, "Sorcery");
                            break;
                        case 5:
                            deckList.setType(i, "Instant");
                            break;
                    }
                }
                //Now time for lands
                else
                {
                    deckList.setType(i, "Land");

                }
                int rndmPick;
                List<string> tempIdentity = new List<string>();

                //set color identity
                if (white)
                {
                    rndmPick = rndm.Next(2);
                    if (rndmPick == 1)
                    {
                        tempIdentity.Add("W");
                    }
                }
                if (blue)
                {
                    rndmPick = rndm.Next(2);
                    if (rndmPick == 1)
                    {
                        tempIdentity.Add("U");
                    }
                }
                if (black)
                {
                    rndmPick = rndm.Next(2);
                    if (rndmPick == 1)
                    {
                        tempIdentity.Add("B");
                    }
                }
                if (red)
                {
                    rndmPick = rndm.Next(2);
                    if (rndmPick == 1)
                    {
                        tempIdentity.Add("R");
                    }
                }
                if (green)
                {
                    rndmPick = rndm.Next(2);
                    if (rndmPick == 1)
                    {
                        tempIdentity.Add("G");
                    }
                }


                string[] identity = tempIdentity.ToArray();

                deckList.setColorIdentity(i, identity);



            }
        }
        private void queryCards(DatabaseWrangler dbWrangler)
        {

            for (int i = 1; i < 100; i++)
            {
                deckList.setCard(i, dbWrangler.queryCard(colorIdentitySearch, deckList.getCard(i)));
            }
        }
    }
         
}
