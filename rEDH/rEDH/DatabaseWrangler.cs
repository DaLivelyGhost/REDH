using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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
        private static string tableDefinitionStrings = "name TEXT, cmc INT, manaCost TEXT, imageURI TEXT, isLegendary BOOLEAN, colorIdentity TEXT";
        private static string tableColumns = "name, cmc, manaCost, imageURI, isLegendary, colorIdentity";
        private static string valuesString = "values ($name, $cmc, $manaCost, $imageURI, $isLegendary, $colorIdentity)";
        private static string foreignKeyString = "FOREIGN KEY (name) REFERENCES Master(name)";
        private void addParameters(Card c)
        {
            command.Parameters.AddWithValue("$name", c.name);
            command.Parameters.AddWithValue("$cmc", c.cmc);
            command.Parameters.AddWithValue("$manaCost", c.mana_cost);
            command.Parameters.AddWithValue("$imageURI", c.image_uris.normal);
            command.Parameters.AddWithValue("$isLegendary", c.isLegendary);
            command.Parameters.AddWithValue("$colorIdentity", c.color_identity_string);
        }

        //Command strings. 
        private static string createString = "CREATE TABLE IF NOT EXISTS ";
        private static string dropString = "DROP TABLE IF EXISTS ";
        private static string selectString = "SELECT " + tableColumns + " FROM ";
        private static string insertString = "INSERT INTO ";

        //Tables. Every card will be inserted into "Master", and then inserted into each table that applies.
        //All tables except "all" has a foreign key that points to name column in "all". This keeps the data normalized.
        private static string masterString = "Master";
        //private static string[] colorStrings = {"White","Blue", "Black", "Red", "Green", "Colorless" };
        private static string[] cardtypeStrings = {"Artifact", "Land", "Creature", "Enchantment", "Planeswalker",
                                                    "Sorcery", "Instant" };
        
        //Constructor
        public DatabaseWrangler()
        {
            string databaseDir = AppDomain.CurrentDomain.BaseDirectory + "Assets\\cardDatabase.db";
            connection = new SqliteConnection("Data Source=" + databaseDir);
            connection.Open();
            
        }

        private void defineTables()
        {

            string commandString;

            //CREATE TABLE IF NOT EXISTS Master ([columns]);
            commandString = createString + masterString + " (" + tableDefinitionStrings + ");";
            command = new SqliteCommand(commandString, connection);
            command.ExecuteNonQuery();

            //CREATE TABLE IF NOT EXISTS [tablename] ([columns] FOREIGN KEY(name) REFERENCES all(name));
            //for (int i = 0; i < colorStrings.Length; i++)
            //{               
            //    commandString = createString + colorStrings[i] + " (" + tableDefinitionStrings + ", " + foreignKeyString + ");";
            //    command = new SqliteCommand(commandString, connection);
            //    command.ExecuteNonQuery();
            //}
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

            //foreach (string color in colorStrings)
            //{
            //    commandString = dropString + color;
            //    command = new SqliteCommand(commandString, connection);
            //    command.ExecuteNonQuery();

            //}
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
            //if color identity does not exist, insert into colorless table, else insert it into color tables
            //if (c.color_identity == null)
            //{
            //    command = new SqliteCommand(insertString + "Colorless" + " (" +
            //tableColumns + ") " + valuesString, connection, transaction);
                
            //    addParameters(c);
            //    command.ExecuteNonQuery();
            //}
            //else
            //{
            //    foreach (string color in c.color_identity)
            //    {
            //        string cardColor = "";

            //        switch (color)
            //        {
            //            case "W":
            //                cardColor = colorStrings[0];
            //                break;
            //            case "U":
            //                cardColor = colorStrings[1];
            //                break;
            //            case "B":
            //                cardColor = colorStrings[2];
            //                break;
            //            case "R":
            //                cardColor = colorStrings[3];
            //                break;
            //            case "G":
            //                cardColor = colorStrings[4];
            //                break;
            //        }

            //        command = new SqliteCommand(insertString + cardColor + " (" +
            //                    tableColumns + ") " + valuesString, connection, transaction);
                    
            //        addParameters(c);
            //        command.ExecuteNonQuery();
            //    }
            //}
        }
        public Card queryCommander(string colorSearchToken)
        {

                string commandString = selectString + cardtypeStrings[2] + " WHERE isLegendary = 1 AND "
                                + colorSearchToken + " ORDER BY RANDOM() LIMIT 1;";

            command = new SqliteCommand(commandString, connection);
            SqliteDataReader reader = command.ExecuteReader();

            Card newCommander = new Card();

            while (reader.Read())
            {
                newCommander.name = reader[0].ToString();
                newCommander.cmc = float.Parse(reader[1].ToString());
                newCommander.mana_cost = reader[2].ToString();
                newCommander.image_uris.normal = reader[3].ToString();
                newCommander.isLegendary = true;
                newCommander.color_identity_string = reader[5].ToString();
            }

            return newCommander;
        }
        public Card queryCard(string colorSearchToken, Card blueprint)
        {
            string cmcString = "cmc == ";
            cmcString += blueprint.cmc.ToString();

            //Append an AND to the end of the color search token. 
            //Not necessary if the user checks all 5 boxes, which results in the search string to be ""
            if (!colorSearchToken.Equals(""))
            {
                colorSearchToken += " AND ";   
            }

            string commandString = selectString + blueprint.card_type.ElementAt(0) + " WHERE " + colorSearchToken
                    + cmcString + " ORDER BY RANDOM() LIMIT 1;";

            command = new SqliteCommand(commandString, connection);
            SqliteDataReader reader = command.ExecuteReader();

            while(reader.Read())
            {
                blueprint.name = reader[0].ToString();
                blueprint.mana_cost = reader[2].ToString();
                blueprint.image_uris.normal = reader[3].ToString();
                if (int.Parse(reader[4].ToString()) == 1)
                {
                    blueprint.isLegendary= true;
                }
                else
                {
                    blueprint.isLegendary = false;
                }
                blueprint.color_identity_string = reader[4].ToString();
            }

            return blueprint;
        }
        public Card getCardByName(string name)
        {
            string fetchCard = selectString;
            Card toReturn = new Card();

            foreach (string s in cardtypeStrings)
            {
                fetchCard += s;
                fetchCard += " WHERE " + " name = '" + name + "';";
                

                command = new SqliteCommand(fetchCard, connection);
                SqliteDataReader reader = command.ExecuteReader();

                while(reader.Read())
                {
                    System.Diagnostics.Debug.WriteLine(reader[0].ToString() + " | " +
                                                        reader[1].ToString() + " | " +
                                                        reader[2].ToString());
                    toReturn.name = reader[0].ToString();
                    toReturn.cmc = float.Parse(reader[1].ToString());
                    toReturn.image_uris.normal = reader[2].ToString();
                }
                

                //reset query string
                fetchCard = selectString;
            }
            System.Diagnostics.Debug.WriteLine(toReturn.name + " / " + toReturn.cmc.ToString()
                                                + " / " + toReturn.image_uris.normal);

            return toReturn;
        }
        public void closeDatabase()
        {
            connection.Close();
        }
    }
}
