using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments.AppointmentsProvider;
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


        static int[] midrangeCurve = {0,1,1,1,1,1,1,2,2,2,2,
                                      2,2,2,2,2,2,2,2,2,2,
                                      3,3,3,3,3,3,3,3,3,3,
                                      3,4,4,4,4,4,4,4,4,4,
                                      4,4,5,5,5,5,5,5,5,5,
                                      5,6,6,6,6,6,6,6,6,7,
                                      7,8};

        static int[] hirangeCurve = {0,0,1,2,2,2,3,3,3,3,3,
                                     3,3,3,3,3,4,4,4,4,4,
                                     4,4,4,4,4,5,5,5,5,5,
                                     5,5,5,5,5,5,5,5,5,6,
                                     6,6,6,6,6,6,6,7,7,7,
                                     7,7,7,7,7,8,8,8,10,10};

        static int[] onesCurve = {0,1,1,1,1,1,1,1,1,1,1,
                                  1,1,1,1,1,1,1,1,1,1,
                                  1,1,1,1,1,1,1,1,1,1,
                                  1,1,1,1,1,1,1,1,1,1,
                                  1,1,1,1,1,1,1,1,1,1,
                                  1,1,1,1,1,1,1,1,1,1,
                                  1,1,1,1,1,1,1,1,1};

        public DeckBuilder()
        {
            deckList = new DeckList();
        }
        public DeckList getDeckList()
        {
            return deckList;
        }
        public async Task<Card[]> buildDeck(DatabaseWrangler dbWrangler, DeckDefinitions definition)
        {

            //establish the format we'll be building.
            dbWrangler.setFormatSearchTerms(definition.format);

            //establish the mana curve we'll be using (only loosely adhered to)
            int[] curve = setManaCurve(definition.manaCurve);


            //Establish Commander first ---------------------------------------------------------------------------
            //I set the commander identity rq here because it absolutely has to include EVERY selected color whereas every other
            //card is optional

            try
            {
                deckList.setCommander(dbWrangler.queryCard(definition.selectedColors, "Creature", 0, true, definition.format));
            }
            catch(Exception ex)
            {
                return null;
            }

            //we're gonna exclude named cards from being generated twice
            dbWrangler.excludeCardNames(deckList.getCard(0).name);
            //-----------------------------------------------------------------------------------------------------

            //Now for the 99

            Random rndm = new Random();
            int random;

            try
            {
                for (int i = 1; i < 100; i++)
                {
                    //grab random color identity out of the possible options
                    string[] cardColorIdentity = setColorIdentity(definition.selectedColors);
                    random = rndm.Next(0, possibleTypes.Length);


                    //create cards on curve of random type and mana value.
                    if (i < curve.Length)
                    {

                        deckList.setCard(i, dbWrangler.queryCard(cardColorIdentity, possibleTypes[random], curve[i], false, definition.format));

                        //if the name is "", then the card doesnt actually exist and will need to be retried until it does.
                        if (deckList.getCard(i).name.Equals(""))
                        {

                            deckList.setCard(i, validateCard(deckList.getCard(i), possibleTypes, cardColorIdentity, curve[i], dbWrangler, definition.format, definition));

                        }

                    }
                    //we are beyond the curve. create lands.
                    else
                    {

                        deckList.setCard(i, dbWrangler.queryCard(cardColorIdentity, "Land", 0, false, definition.format));

                        //if somehow creating a land fails (don't ask me i'm just the programmer):
                        if (deckList.getCard(i).name.Equals(""))
                        {
                            deckList.setCard(i, validateCard(deckList.getCard(i), possibleTypes, cardColorIdentity, 0, dbWrangler, definition.format, definition));
                        }
                    }
                    dbWrangler.excludeCardNames(deckList.getCard(i).name);
                }
            }
            catch(Exception ex)
            {
                return null;
            }
            finally
            {
                //reset search terms when done.
                dbWrangler.resetSearchTerms();
            }

            //return to display.
            return deckList.getDeck();
        }
        private string[] setColorIdentity(string[] chosenColors)
        {
            Random rndm = new Random();
            int rndmPick;

            List<string> tempIdentity = new List<string>();

            //50% random chance to add each color in the color identity to the color identity of this card.
            foreach(string color in chosenColors)
            {
                rndmPick = rndm.Next(0, 2);
                if(rndmPick == 1)
                {
                    tempIdentity.Add(color);
                }
            }

            return tempIdentity.ToArray();
        }
        private int[] setManaCurve(string manaCurve)
        {
            int[] emptyCurve = new int[0];

            switch (manaCurve)
            {
                case "Low Range":
                    emptyCurve = lowrangeCurve;
                    break;
                case "Mid Range":
                    emptyCurve = midrangeCurve;
                    break;
                case "High Range":
                    emptyCurve = hirangeCurve;
                    break;
                case "Oops all 1's!":
                    emptyCurve = onesCurve;
                    break;

            }
            return emptyCurve;
        }
        private Card validateCard(Card toValidate, string[] possibleTypes, string[] colorIdentity, 
            int cmc, DatabaseWrangler dbWrangler, string format, DeckDefinitions definition)
        {

            //try to test each card type against the current color pie for the card
            toValidate = testCardTypes(toValidate, dbWrangler, toValidate.color_identity, possibleTypes, format);

            //if that failed, then we need to go through the colors in the identity individually
            if(toValidate.name.Equals(""))
            {
                foreach(string color in definition.selectedColors)
                {
                    string[] colorAsArray = { color };
                    toValidate = testCardTypes(toValidate, dbWrangler, colorAsArray, possibleTypes, format);

                    if(!toValidate.name.Equals(""))
                    {
                        return toValidate;
                    }
                }
                //if we made it here, it means that everything has failed and we need to just generate a card.
                toValidate = dbWrangler.queryCard(toValidate.color_identity, null, cmc, false, format);
            }

            return toValidate;
        }

        private Card testCardTypes(Card toValidate, DatabaseWrangler dbWrangler, string[] color, string[] possibleTypes,string format)
        {
            //since this is a recursive program, we need to catch 
            if(possibleTypes.Length == 0)
            {
                return toValidate;
            }

            //pick a random type to test
            Random random = new Random();
            int randomType = random.Next(0,possibleTypes.Length);

            toValidate = dbWrangler.queryCard(color, possibleTypes[randomType], toValidate.cmc, false, format);


            //if it failed, then it's time to get recursive baybeeeee
            if(toValidate.name.Equals(""))
            {
                //remove the tested type from the array
                List<string> remainingTypes = new List<string>(possibleTypes);
                remainingTypes.RemoveAt(randomType);

                toValidate = testCardTypes(toValidate, dbWrangler, color, remainingTypes.ToArray(), format);
            }

            return toValidate;
        }
    }
         
}
