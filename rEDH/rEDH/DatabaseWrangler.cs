using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;

namespace rEDH
{
    public class DatabaseWrangler
    {
        private SqliteConnection connection;
        private SqliteCommand command;

        private static string tableDefinitionStrings = " (name TEXT, cmc INT, imageURI TEXT)";
        private static string tableColumns = "name, cmc, imageURI";

        private static string createString = "CREATE TABLE IF NOT EXISTS ";
        private static string selectString = "SELECT " + tableColumns + " FROM ";

        private static string[] colorStrings = {"white","blue", "black", "red", "green" };
        private static string[] cardtypeStrings = {"Artifact", "Land", "Creature", "Enchantment", "Planeswalker",
                                                    "Sorcery", "Instant" };
        
        public DatabaseWrangler()
        {
            connection = new SqliteConnection("Data Source=cardDatabase.db");
            connection.Open();

            defineTables();
        }

        private void defineTables()
        {
            string commandString;

            for(int i = 0; i < colorStrings.Length; i++)
            {
                commandString = createString + colorStrings[i] + tableDefinitionStrings + ";";
                command = new SqliteCommand(commandString, connection);
                command.ExecuteNonQuery();
            }
            for (int i = 0; i < cardtypeStrings.Length; i++)
            {
                commandString = createString + cardtypeStrings[i] + tableDefinitionStrings + ";";
                command = new SqliteCommand(commandString, connection);
                command.ExecuteNonQuery();
            }
        }
        public void addCard(Card toAdd)
        {
            string commandString = "INSERT INTO ";
            string valuesString = " values ($name, $cmc, $imageURI)";
            
            foreach (string s in toAdd.card_type)
            {
                command = new SqliteCommand(commandString + s + "(" + tableColumns + ")" + valuesString, connection);
                command.Parameters.AddWithValue("$name", toAdd.name);
                command.Parameters.AddWithValue("$cmc", toAdd.cmc);
                command.Parameters.AddWithValue("$imageURI", toAdd.image_uris.normal);
                command.ExecuteNonQuery();
            }
        }
        public Card getCardByName(string name)
        {
            string fetchCard = selectString;

            foreach (string s in cardtypeStrings)
            {
                fetchCard += s;
                fetchCard += " WHERE " + s + " name = '" + name + "';";
                //command.Parameters.AddWithValue("'$name'", name);

                command = new SqliteCommand(fetchCard, connection);
                SqliteDataReader reader = command.ExecuteReader();

                while(reader.Read())
                {
                    System.Diagnostics.Debug.WriteLine(reader[0].ToString() + " | " +
                                                        reader[1].ToString() + " | " +
                                                        reader[2].ToString());
                }    
                
                //reset query string
                fetchCard = selectString;
            }          

            return null;
        }
        public void closeDatabase()
        {
            connection.Close();
        }
    }
}
