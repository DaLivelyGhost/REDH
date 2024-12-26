using Microsoft.Data.Sqlite;
using Microsoft.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Windows.Security.Cryptography.Core;
using Windows.UI.Input.Inking;
using Windows.Web.UI;

namespace rEDH
{   
    /// <summary>
    ///  Object that handles the local database of cards.
    /// </summary>
    public class DatabaseWrangler
    {
        private SqliteConnection connection;
        private SqliteCommand command;
        private SqliteTransaction transaction;

        //Columns. If add/remove columns do it here.
        private static string tableDefinitionStrings = "name TEXT, cmc INT, manaCost TEXT, imageURI TEXT, isLegendary BOOLEAN, " +
            "colorIdentity TEXT, cardType TEXT, edhLegal TEXT, pauperLegal TEXT, predhLegal TEXT, rarity TEXT";
        private static string tableColumns = "name, cmc, manaCost, imageURI, isLegendary, colorIdentity, cardType, edhLegal, pauperLegal, predhLegal, rarity";
        private static string valuesString = "values ($name, $cmc, $manaCost, $imageURI, $isLegendary, $colorIdentity, $cardType, $edhLegal, $pauperLegal, $predhLegal, $rarity)";
        private static string foreignKeyString = "FOREIGN KEY (name) REFERENCES Master(name)";
        private void addParameters(Card c)
        {
            command.Parameters.AddWithValue("$name", c.name);
            command.Parameters.AddWithValue("$cmc", c.cmc);
            command.Parameters.AddWithValue("$manaCost", c.mana_cost);
            command.Parameters.AddWithValue("$imageURI", c.image_uris.normal);
            command.Parameters.AddWithValue("$isLegendary", c.isLegendary);
            command.Parameters.AddWithValue("$colorIdentity", c.color_identity_string);
            command.Parameters.AddWithValue("$cardType", concatonateString(c.card_type.ToArray()));
            command.Parameters.AddWithValue("$edhLegal", c.legalities.commander);
            command.Parameters.AddWithValue("$pauperLegal", c.legalities.paupercommander);
            command.Parameters.AddWithValue("$predhLegal", c.legalities.predh);
            command.Parameters.AddWithValue("$rarity", c.rarity);
        }

        //Command strings. 
        private static string createString = "CREATE TABLE IF NOT EXISTS ";
        private static string dropString = "DROP TABLE IF EXISTS ";
        private static string selectString = "SELECT " + tableColumns + " FROM ";
        private static string insertString = "INSERT INTO ";

        private string formatTokenString = "";
        
        //command strings & variables for enforcing singleton.
        private string cardExclusionsString = "";
        private List<string> excludedNames = new List<string>();
        private int excludedCardsCount = 0;

        //Tables. Every card will be inserted into "Master", and then inserted into each table that applies.
        //All tables except "all" has a foreign key that points to name column in "all". This keeps the data normalized.
        private static string masterString = "Master";
        private static string[] cardtypeStrings = {"Artifact", "Land", "Creature", "Enchantment", "Planeswalker",
                                                    "Sorcery", "Instant" };
        
        //Constructor
        public DatabaseWrangler()
        {

            
        }
        public void createConnection()
        {
            string databaseDir = AppDomain.CurrentDomain.BaseDirectory + "Assets\\cardDatabase.db";
            connection = new SqliteConnection("Data Source=" + databaseDir);
            connection.Open();

            defineTables();
        }

        private void defineTables()
        {

            string commandString;

            //CREATE TABLE IF NOT EXISTS Master ([columns]);
            commandString = createString + masterString + " (" + tableDefinitionStrings + ");";
            command = new SqliteCommand(commandString, connection);
            command.ExecuteNonQuery();

            for (int i = 0; i < cardtypeStrings.Length; i++)
            {
                commandString = createString + cardtypeStrings[i] + " (" +  tableDefinitionStrings + ", " + foreignKeyString + ");";
                command = new SqliteCommand(commandString, connection);
                command.ExecuteNonQuery();
            }

            commandString = "CREATE TABLE IF NOT EXISTS " +
                            "AppInfo (lastUpdated TEXT);";
            command = new SqliteCommand(commandString, connection);
            command.ExecuteNonQuery();

        }
        private void dropTables()
        {

            string commandString;

            foreach (string type in cardtypeStrings)
            {
                commandString = dropString + type;
                command = new SqliteCommand(commandString, connection);
                command.ExecuteNonQuery();

            }

            commandString = dropString + masterString;
            command = new SqliteCommand(commandString, connection);
            command.ExecuteNonQuery();

            commandString = dropString + "AppInfo";
            command = new SqliteCommand(commandString, connection);
            command.ExecuteNonQuery();
        }
        public async void setTimeUpdated(string time)
        {
            string commandString = insertString + "AppInfo" + " (lastUpdated) values($lastUpdated)";

            command = new SqliteCommand(commandString, connection);
            command.Parameters.AddWithValue("$lastUpdated", time);
            command.ExecuteNonQuery();

        }
        public string getTimeUpdated()
        {
            try
            {
                command = new SqliteCommand("SELECT lastUpdated FROM AppInfo", connection);

                SqliteDataReader reader = command.ExecuteReader();
                string lastUpdated = "";

                while (reader.Read())
                {
                    lastUpdated = reader[0].ToString();
                }
                return lastUpdated;
            }
            catch(Exception e)
            {
                return null;
            }

        }
        public async void refreshTables()
        {
            dropTables();
            defineTables();
        }
        public async void addCardsBulk(Card[] cardList)
        {

            //temporarily disable enforcement of referential integrity so we can add cards to parent table
            //and child tables at the same time. Normally referential integrity forces a primary key to be in 
            //place before the foreign key.
            command = new SqliteCommand("PRAGMA foreign_keys = OFF", connection);
            command.ExecuteNonQuery();

            transaction = connection.BeginTransaction();

            foreach (Card c in cardList)
            {
                //gotta filter out cards that don't actually go in your deck, like tokens
                //FOR THE TIME BEING: I'm blocking 2 sided cards until I get around to dealing with their formatting.
                if (!c.layout.Equals("art_series") && !c.layout.Equals("double_faced_token") && !c.layout.Equals("scheme")
                        && !c.layout.Equals("emblem") && !c.layout.Equals("token") && !c.layout.Equals("vanguard")
                         && !c.layout.Equals("augment") && !c.layout.Equals("meld") && !c.layout.Equals("transform")
                         && !c.layout.Equals("modal_dfc"))
                {
                    insertCard(c);
                }
            }
            transaction.Commit();

            //re-enable referential integrity.
            command = new SqliteCommand("PRAGMA foreign_keys = ON", connection);
            command.ExecuteNonQuery();

        }
        public void insertCard(Card c)
        {
            //insert into master
            command = new SqliteCommand(insertString + masterString + " (" +
                                        tableColumns + ") " + valuesString, connection, transaction);

            addParameters(c);
            command.ExecuteNonQuery();

            //insert into card type tables.
            foreach(string type in c.card_type)
            {
                command = new SqliteCommand(insertString + type + " (" +
                            tableColumns + ") " + valuesString, connection, transaction);
                
                addParameters(c);
                command.ExecuteNonQuery();
            }
        }
        
        public Card queryCard(string[] colorIdentity, string cardType, float cmc, bool isCommander, string format)
        {

            
            //create a search token based off the colors that are in the card's color identity.
            //ex. WHERE colorIdentity LIKE 'G' and colorIdentity LIKE 'B'
            string colorSearchToken = setColorSearchTerms(colorIdentity);

            string commandString = "";
            command = new SqliteCommand(null, connection);

            //if its our commander, then its the very first card we are adding to the deck, so we don't care about
            //cmc or making sure that it doesn't conflict with another card.
            if (isCommander)
            {
                if(format.Equals("Pauper Commander"))
                {
                    commandString = selectString + "Creature" + " WHERE rarity LIKE 'uncommon' AND "
                        + colorSearchToken + " ORDER BY RANDOM() LIMIT 1;";
                }
                else
                {
                    commandString = selectString + "Creature" + " WHERE isLegendary = 1 AND "
                    + colorSearchToken + " AND " + formatTokenString + " ORDER BY RANDOM() LIMIT 1;";
                }

            }
            //The 99
            else if(cardType != null)
            {
                //if generating a pauper deck, generate only common cards
                //Yes, I know there are some cards that are uncommon that used to be common, but im tired and have no way of verifying those
                if(format.Equals("Pauper Commander"))
                {
                    commandString = selectString + cardType + " WHERE "
                    + colorSearchToken + " AND " + "cmc = " + cmc.ToString()
                    + " AND " + "rarity LIKE 'common'"
                    + " AND " + cardExclusionsString + " AND " + formatTokenString
                    + " ORDER BY RANDOM() LIMIT 1;";
                }
                else
                {
                    commandString = selectString + cardType + " WHERE "
                    + colorSearchToken + " AND " + "cmc = " + cmc.ToString() 
                    + " AND " + cardExclusionsString + " AND " + formatTokenString
                    + " ORDER BY RANDOM() LIMIT 1;";
                }
                addExcludedCardParameters();
            }
            //edge case for when validateCard has hit a wall. Generates a card in the format and color identity.
            //Shows no care for type or cmc.
            else
            {
                //generate a common card if working in a pauper commander space.
                if(format.Equals("Pauper Commander"))
                {
                    commandString = selectString + masterString + " WHERE " + colorSearchToken
                    + " AND " + cardExclusionsString + " AND " + formatTokenString 
                    + " AND " + "rarity LIKE 'common'" + " ORDER BY RANDOM() LIMIT 1;";
                }
                else
                {
                    commandString = selectString + masterString + " WHERE " + colorSearchToken
                    + " AND " + cardExclusionsString + " AND " + formatTokenString 
                    + " ORDER BY RANDOM() LIMIT 1;";
                }

                addExcludedCardParameters();
            }

            addFormatParameters();

            command.CommandText = commandString;

            command.Prepare();
            SqliteDataReader reader;

            reader = command.ExecuteReader();

            Card newCard = new Card();

            while (reader.Read())
            {
                //name
                newCard.name = reader[0].ToString();
                //cmc
                string cmcString = reader[1].ToString();
                newCard.cmc = float.Parse(cmcString);
                //manacost                
                newCard.mana_cost = reader[2].ToString();
                //image URIs
                newCard.image_uris.normal = reader[3].ToString();
                //isLegendary
                if (int.Parse(reader[4].ToString()) == 1)
                {
                    newCard.isLegendary = true;
                }
                else
                {
                    newCard.isLegendary = false;
                }
                //color identity
                newCard.color_identity_string = reader[5].ToString();
                newCard.color_identity = deconcatonateChars(reader[5].ToString());
                //type
                string type = reader[6].ToString();
                newCard.card_type = deconcatonateString(type);
                //legalities
                newCard.legalities.commander = reader[7].ToString();
                newCard.legalities.paupercommander = reader[8].ToString();
                newCard.legalities.predh = reader[9].ToString();
                //rarity
                newCard.rarity = reader[10].ToString();
            }

            return newCard;
        }
        private string setColorSearchTerms(string[] colorIdentity)
        {
            //if the card is colorless, building a string is simple
            if(colorIdentity.Length == 0 || colorIdentity[0].Equals(""))
            {
                return "colorIdentity LIKE ''";
            }
            //if the card is monocolored, the string is a bit more complex, but still painless.
            if(colorIdentity.Length == 1)
            {
                string monoColor = "colorIdentity LIKE ";
                
                switch (colorIdentity[0]) {
                    case "W":
                        monoColor += "'W'";
                        break;
                    case "U":
                        monoColor += "'U'";
                        break;
                    case "B":
                        monoColor += "'B'";
                        break;
                    case "R":
                        monoColor += "'R'";
                        break;
                    case "G":
                        monoColor += "'G'";
                        break;
                }
                return monoColor;
            }
            //if we make it here, it means that the card is multi colored and now we gotta do actual work :/
            string multiColored = "";

            if(colorIdentity.Contains("W"))
            {
                multiColored += "colorIdentity LIKE '%W%'";
            }
            else
            {
                multiColored += "colorIdentity NOT LIKE '%W%'";
            }

            multiColored += " AND ";

            if (colorIdentity.Contains("U"))
            {
                multiColored += "colorIdentity LIKE '%U%'";
            }
            else
            {
                multiColored += "colorIdentity NOT LIKE '%U%'";
            }

            multiColored += " AND ";

            if (colorIdentity.Contains("B"))
            {
                multiColored += "colorIdentity LIKE '%B%'";
            }
            else
            {
                multiColored += "colorIdentity NOT LIKE '%B%'";
            }

            multiColored += " AND ";

            if (colorIdentity.Contains("R"))
            {
                multiColored += "colorIdentity LIKE '%R%'";
            }
            else
            {
                multiColored += "colorIdentity NOT LIKE '%R%'";
            }

            multiColored += " AND ";

            if (colorIdentity.Contains("G"))
            {
                multiColored += "colorIdentity LIKE '%G%'";
            }
            else
            {
                multiColored += "colorIdentity NOT LIKE '%G%'";
            }

            return multiColored;
        }
        public void setFormatSearchTerms(string format)
        {
            if(format == null)
            {
                formatTokenString = "edhLegal LIKE $legal";
            }
            switch(format)
            {
                case ("EDH"):

                    formatTokenString = "edhLegal LIKE $legal";
                    break;
                case ("PreDH"):

                    formatTokenString = "predhLegal LIKE $legal";
                    break;
                case ("Pauper Commander"):

                    formatTokenString =  "pauperLegal LIKE $legal";
                    break;

            }
        }
        private void addFormatParameters()
        {
            SqliteParameter formatParam = new SqliteParameter("$legal", SqliteType.Text);
            formatParam.Value = "legal";
            command.Parameters.Add(formatParam);
        }
        public void excludeCardNames(string toExclude)
        {
            
            //TODO maybe try reading the string to add a \ before any apostrophes in the name?
            if(cardExclusionsString.Equals(""))
            {
                cardExclusionsString = "name NOT LIKE $name";
            }
            else
            {
                cardExclusionsString += " AND name NOT LIKE $name";
            }
            //cardExclusionsString += toExclude + "'";
            cardExclusionsString += excludedCardsCount.ToString();
            excludedNames.Add(toExclude);

            excludedCardsCount++;
        }
        private void addExcludedCardParameters()
        {
            for(int i = 0; i < excludedCardsCount; i++)
            {
                SqliteParameter nameParam = new SqliteParameter("$name" + i.ToString(), SqliteType.Text);
                nameParam.Value = excludedNames.ElementAt(i);
                command.Parameters.Add(nameParam);
            }
        }
        private string concatonateString(string[] toConcatonate)
        {
            string toReturn = "";
            for (int i = 0; i < toConcatonate.Length; i++)
            {
                if(i > 0)
                {
                    toReturn += ",";
                }
                toReturn += toConcatonate[i];
            }
            return toReturn;
        }
        private List<string> deconcatonateString(string toDeconcatonate)
        {
            char[] charArray = toDeconcatonate.ToCharArray();
            List<char> charList = new List<char>();
            List<string> toReturn = new List<string>();

            //step through the string as a list of chars and break whenever we hit a comma
            for(int i = 0; i < charArray.Length; i++)
            {
                if (charArray[i].Equals(','))
                {
                    string type = "";
                    foreach(char c in charList)
                    {
                        type += c;
                    }
                    toReturn.Add(type);
                    charList.Clear();
                }
                else
                {
                    charList.Add(charArray[i]);
                }
            }
            //if toReturn is empty, then it means we did not hit a comma. (singular type)
            if(toReturn.Count() == 0)
            {
                string type = "";
                foreach(char c in charList)
                {
                    type += c;
                }

                toReturn.Add(type);
            }

            return toReturn;
        }
        private string[] deconcatonateChars(string toDeconcatonate)
        {
            char[] charArray = toDeconcatonate.ToCharArray();
            string[] toReturn = new string[charArray.Length];

            for (int i = 0; i < charArray.Length; i++)
            {
                toReturn[i] = charArray[i].ToString();
            }

            return toReturn;
        }
        public void resetSearchTerms()
        {
            cardExclusionsString = "";
        }
        public void closeDatabase()
        {
            connection.Close();
        }
    }
}
