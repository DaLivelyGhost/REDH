using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;

namespace rEDH
{
    internal class DeckBuilder
    {
        DeckList deckList;

        static string[] possibleTypes = { "Artifact", "Creature", "Enchantment", "Instant", "Land", "Planeswalker", "Sorcery" };
        
        //The integers represent cmc. Example: each 1 is a card that costs 1 mana.
        //0 is the commander. Everything after this array will be lands.
        static int[] lowrangeCurve = {0,1,1,1,1,1,1,1,1,1,2,
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
        string singletonCards;
        public DeckBuilder() 
        {
            deckList = new DeckList();
        }
        public DeckList getDeckList()
        {
            return deckList;
        }
        public Card[] buildDeck(DatabaseWrangler dbWrangler, bool white, bool blue, bool black, bool red, bool green)
        {

            bool[] chosenColors = { white, blue, black, red, green };
            

            //Establish Commander first ---------------------------------------------------------------------------
            List<string> commanderIdentity = new List<string>();
            if(white)
            {
                commanderIdentity.Add("W");
            }
            if(blue)
            {
                commanderIdentity.Add("U");
            }
            if (black)
            {
                commanderIdentity.Add("B");
            }
            if (red)
            {
                commanderIdentity.Add("R");
            }
            if(green)
            {
                commanderIdentity.Add("G");
            }

            deckList.setCommander(dbWrangler.queryCard(commanderIdentity.ToArray(), "Creature", 0, true));

            //we're gonna exclude named cards from being generated twice
            dbWrangler.excludeCardNames(deckList.getCard(0).name);
            //-----------------------------------------------------------------------------------------------------
            
            //Now for the 99
            
            Random rndm = new Random();
            int random;

            for (int i = 1; i < 100; i++)
            {
                
                string[] cardColorIdentity = setColorIdentity(chosenColors);

                random = rndm.Next(0,possibleTypes.Length);
               
                //create cards on curve of random type and mana value.
                if(i < lowrangeCurve.Length)
                {
                    deckList.setCard(i, dbWrangler.queryCard(cardColorIdentity, possibleTypes[random], lowrangeCurve[i], false));
                }
                //we are beyond the curve. create lands.
                else
                {
                    deckList.setCard(i, dbWrangler.queryCard(cardColorIdentity, "Land", 0, false));
                }

                //if the name is "", then the card doesnt actually exist and will need to be retried until it does.
                if(deckList.getCard(i).name.Equals(""))
                {
                    
                    deckList.setCard(i, validateCard(deckList.getCard(i), possibleTypes, dbWrangler));
                }

                dbWrangler.excludeCardNames(deckList.getCard(i).name);
            }

            //return to display.
            return deckList.getDeck();
        }
        private string[] setColorIdentity(bool[] chosenColors)
        {
            Random rndm = new Random();
            int rndmPick;

            List<string> tempIdentity = new List<string>();

            //white
            if (chosenColors[0])
            {
                rndmPick = rndm.Next(2);
                if (rndmPick == 1)
                {
                    tempIdentity.Add("W");
                }
            }
            //blue
            if (chosenColors[1])
            {
                rndmPick = rndm.Next(2);
                if (rndmPick == 1)
                {
                    tempIdentity.Add("U");
                }
            }
            //black
            if (chosenColors[2])
            {
                rndmPick = rndm.Next(2);
                if (rndmPick == 1)
                {
                    tempIdentity.Add("B");
                }
            }
            //red
            if (chosenColors[3])
            {
                rndmPick = rndm.Next(2);
                if (rndmPick == 1)
                {
                    tempIdentity.Add("R");
                }
            }
            //green
            if (chosenColors[4])
            {
                rndmPick = rndm.Next(2);
                if (rndmPick == 1)
                {
                    tempIdentity.Add("G");
                }
            }

            return tempIdentity.ToArray();
        }
        private Card validateCard(Card toValidate, string[] possibleTypes, DatabaseWrangler dbWrangler)
        {
            //possibleTypes is all the main card types a card can be.
            //we'll be narrowing down the list until it generates a card that exists.
            string[] currentOptions = possibleTypes;
            List<string> cardTypes = new List<string>(currentOptions);
            

            //pick a random card type
            Random rndm = new Random();
            int random = rndm.Next(0, cardTypes.Count);

            //try again at generating this card.
            toValidate = dbWrangler.queryCard(toValidate.color_identity, cardTypes.ElementAt(random), toValidate.cmc, false);

            if (toValidate.name.Equals(""))
            {
                cardTypes.Remove(toValidate.card_type[0]);
                validateCard(toValidate, cardTypes.ToArray(), dbWrangler);
            }

            return toValidate;
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
                commanderColorIdentitySearch = "";
                colorIdentitySearch = "";

                for (int i = 0; i < colors.Length; i++)
                {
                    //insert an AND inbetween each search token
                    if (!commanderColorIdentitySearch.Equals(""))
                    {
                        commanderColorIdentitySearch += " AND ";
                    }
                    //---------------------------------------------

                    //if the checkbox is unmarked
                    if (!colors[i])
                    {
                        //insert an AND inbetween each search token. The regular search token uses only negative logic like
                        //so the colorIdentitySearch is isolated to unmarked checkboxes. ex: cis NOT LIKE %U% AND NOT LIKE %R%
                        if (!colorIdentitySearch.Equals(""))
                        {
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
                    //if the checkbox is marked
                    else
                    {
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
                }
            }
        }

    }
         
}
