using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        static int[] midrangeCurve = { };
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
        public Card[] buildDeck(DatabaseWrangler dbWrangler, bool white, bool blue, bool black, bool red, bool green, string format)
        {

            bool[] chosenColors = { white, blue, black, red, green };

            //establish the format we'll be building.
            dbWrangler.setFormatSearchTerms(format);

            //Establish Commander first ---------------------------------------------------------------------------
            List<string> commanderIdentity = new List<string>();
            if (white)
            {
                commanderIdentity.Add("W");
            }
            if (blue)
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
            if (green)
            {
                commanderIdentity.Add("G");
            }

            deckList.setCommander(dbWrangler.queryCard(commanderIdentity.ToArray(), "Creature", 0, true, format));

            //we're gonna exclude named cards from being generated twice
            dbWrangler.excludeCardNames(deckList.getCard(0).name);
            //-----------------------------------------------------------------------------------------------------

            //Now for the 99

            Random rndm = new Random();
            int random;

            for (int i = 1; i < 100; i++)
            {

                string[] cardColorIdentity = setColorIdentity(chosenColors);

                random = rndm.Next(0, possibleTypes.Length);
                

                //create cards on curve of random type and mana value.
                if (i < lowrangeCurve.Length)
                {
                    Debug.WriteLine("Writing card...");
                    try
                    {
                        deckList.setCard(i, dbWrangler.queryCard(cardColorIdentity, possibleTypes[random], lowrangeCurve[i], false, format));
                    }
                    catch(Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                    
                }
                //we are beyond the curve. create lands.
                else
                {
                    deckList.setCard(i, dbWrangler.queryCard(cardColorIdentity, "Land", 0, false, format));
                }

                //if the name is "", then the card doesnt actually exist and will need to be retried until it does.
                if (deckList.getCard(i).name.Equals("") || deckList.getCard(i).card_type.Count() == 0)
                {

                    deckList.setCard(i, validateCard(deckList.getCard(i), possibleTypes, dbWrangler, format));
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
        private Card validateCard(Card toValidate, string[] possibleTypes, DatabaseWrangler dbWrangler, string format)
        {
            //if this is true, we've run out of options. There are no more cardtypes in the cmc that
            //will yield results.
            if (possibleTypes.Length == 0)
            {
                //generate a random card in our color affinity and format. Who cares anymore.
                toValidate = dbWrangler.queryCard(toValidate.color_identity, null, -1, false, format);
                return toValidate;
            }

            //possibleTypes is all the main card types a card can be.
            //we'll be narrowing down the list until it generates a card that exists.
            string[] currentOptions = possibleTypes;
            List<string> cardTypes = new List<string>(currentOptions);

            //pick a random card type
            Random rndm = new Random();
            int random = rndm.Next(0, cardTypes.Count);

            //try again at generating this card.
            toValidate = dbWrangler.queryCard(toValidate.color_identity, cardTypes.ElementAt(random), toValidate.cmc, false, format);

            if (toValidate.name.Equals(""))
            {

                cardTypes.RemoveAt(random);
                validateCard(toValidate, cardTypes.ToArray(), dbWrangler, format);

            }

            return toValidate;
        }
    }
         
}
