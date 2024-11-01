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

            //check integrity of decklist, make sure it didn't try to generate any cards that don't exist
            //or duplicates of cards that aren't basic lands.
            validateCards(dbWrangler);
            //ensureSingleton(dbWrangler);

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
                            deckList.setTypeLine(i, "Artifact");
                            break;
                        case 1:
                            deckList.setTypeLine(i, "Creature");
                            break;
                        case 2:
                            deckList.setTypeLine(i, "Enchantment");
                            break;
                        case 3:
                            deckList.setTypeLine(i, "Planeswalker");
                            break;
                        case 4:
                            deckList.setTypeLine(i, "Sorcery");
                            break;
                        case 5:
                            deckList.setTypeLine(i, "Instant");
                            break;
                    }
                }
                //Now time for lands
                else
                {
                    deckList.setTypeLine(i, "Land");

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
        private void validateCards(DatabaseWrangler dbWrangler)
        {
            //iterate through each card in the deck
            for(int i = 0; i < 100; i++)
            {

                //if the name is "", that means that it's not a valid card and needs to be replaced
                if(deckList.getCard(i).name == "")
                {
                    //Possible card types that the card can be. We'll be removing them one by one until a card works.
                    string[] types = {"Artifact","Creature", "Enchantment", "Instant", "Planeswalker", "Sorcery"};
                    List<string> cardTypes = new List<string>(types);

                    while (deckList.getCard(i).name == "")
                    {

                        //since the card doesn't exist, we know it's a blueprinted card and only has 1 card type at the moment.
                        //We'll now remove it from the possible card types it can be and try again.
                        switch (deckList.getCard(i).card_type[0])
                        {
                            case "Artifact":
                                cardTypes.Remove("Artifact");
                                break;
                            case "Creature":
                                cardTypes.Remove("Creature");
                                break;
                            case "Enchantment":
                                cardTypes.Remove("Enchantment");
                                break;
                            case "Instant":
                                cardTypes.Remove("Instant");
                                break;
                            case "Planeswalker":
                                cardTypes.Remove("Planeswalker");
                                break;
                            case "Sorcery":
                                cardTypes.Remove("Sorcery");
                                break;
                        }

                        Random rndm = new Random();
                        int randomType;

                        randomType = rndm.Next(cardTypes.Count());
                        deckList.setTypeLine(i, cardTypes.ElementAt(randomType));
                        deckList.setCard(i, dbWrangler.queryCard(colorIdentitySearch, deckList.getCard(i)));
                    }
                }
            }
        }
        private void ensureSingleton(DatabaseWrangler dbWrangler)
        {
            //starting at 1 to ignore the commander.
            for(int i = 1; i < 100; i++)
            {
                for(int j = i + 1; j < 100; j++)
                {
                    while(deckList.getCard(j).name == deckList.getCard(i).name)
                    {
                        deckList.setCard(j, dbWrangler.queryCard(colorIdentitySearch, deckList.getCard(j)));
                    }
                }
            }
        }
    }
         
}
