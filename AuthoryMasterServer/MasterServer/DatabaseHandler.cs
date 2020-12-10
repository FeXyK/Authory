using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace AuthoryMasterServer
{
    /// <summary>
    /// Communicates with the databasse server. 
    /// Creates queries for the database server.
    /// </summary>
    public class DatabaseHandler
    {
        private string ConnectionString;
        public string DBServer { get; set; }
        public string DBName { get; set; }
        public string DBUsername { get; set; }
        public string DBPassword { get; set; }

        public DatabaseHandler(string dbServer, string dbName, string dbUsername, string dbPassword)
        {
            Console.WriteLine("Initializing database handler...");

            DBServer = dbServer;
            DBName = dbName;
            DBUsername = dbUsername;
            DBPassword = dbPassword;

            ConnectionString = string.Format($"Server={DBServer}; database={DBName}; UID={DBUsername}; password={DBPassword}");

            Console.WriteLine("DatabaseHandler initialized.");
        }

        /// <summary>
        /// Reads an account by its name (not case sensitive) and the password (case sensitive)
        /// </summary>
        /// <param name="name">Name of the account</param>
        /// <param name="password">Password of the account</param>
        /// <returns></returns>
        public Account ReadAccount(string name, string password)
        {
            Console.WriteLine("Reading account...");
            Account account = null;

            MySqlConnection conn = new MySqlConnection(ConnectionString);

            try
            {
                conn.Open();

                string command = "SELECT id FROM authory.account WHERE name LIKE ?name AND password LIKE BINARY ?password";
                MySqlCommand cmd = new MySqlCommand(command, conn);

                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("password", password);

                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    account = new Account(reader.GetInt32(0), name);
                    Console.WriteLine("Reading account done.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine(account);

            return account;
        }

        /// <summary>
        /// Insert an account to the database.
        /// </summary>
        /// <param name="username">Name</param>
        /// <param name="password">Password</param>
        /// <returns>The inserted account's ID</returns>
        public int CreateAccount(string username, string password)
        {
            Console.WriteLine("Creating account...");
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            int id;
            try
            {
                conn.Open();

                string command = "INSERT INTO authory.account(name, password) VALUES(?name, ?password); SELECT last_insert_id();";

                MySqlCommand cmd = new MySqlCommand(command, conn);

                cmd.Parameters.AddWithValue("name", username);
                cmd.Parameters.AddWithValue("password", password);

                id = Convert.ToInt32(cmd.ExecuteScalar());

                Console.WriteLine($"Account created.\nId: {id}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }

            return id;
        }

        /// <summary>
        /// Deletes a character from the database, by accountID, characterID, and name
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="characterId"></param>
        /// <param name="characterName"></param>
        /// <returns>Affected rows in the database</returns>
        public int DeleteCharacter(int accountId, int characterId, string characterName)
        {
            Console.WriteLine($"Deleting character AccId: {accountId}, ChId: {characterId}, Name: {characterName}");
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            int affectedRows = 0;

            try
            {
                conn.Open();

                string command = "DELETE FROM authory.character WHERE account_id=?account_id AND id=?id AND STRCMP(name, ?name);";

                MySqlCommand cmd = new MySqlCommand(command, conn);

                cmd.Parameters.AddWithValue("account_id", accountId);
                cmd.Parameters.AddWithValue("id", characterId);
                cmd.Parameters.AddWithValue("name", characterName);

                affectedRows = cmd.ExecuteNonQuery();
                Console.WriteLine($"Character Deleted\nAffected rows: {affectedRows}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return affectedRows;
        }

        /// <summary>
        /// Inserts a character into the database.
        /// </summary>
        /// <param name="accountId">AccountID of the character</param>
        /// <param name="name">Name of the character</param>
        /// <param name="model">ModelType of the character</param>
        /// <returns>Inserted CharacterID</returns>
        public int CreateCharacter(int accountId, string name, int model)
        {
            Console.WriteLine("Creating character...");

            MySqlConnection conn = new MySqlConnection(ConnectionString);
            int id;

            try
            {
                conn.Open();

                string command = "INSERT INTO authory.character(account_id, name, model) VALUES(?account_id, ?name, ?model); SELECT last_insert_id();";

                MySqlCommand cmd = new MySqlCommand(command, conn);

                cmd.Parameters.AddWithValue("account_id", accountId);
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("model", model);

                id = Convert.ToInt32(cmd.ExecuteScalar());

                Console.WriteLine($"Character created.\nId: {id}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }

            return id;
        }

        /// <summary>
        /// Reads the characters of the account.
        /// </summary>
        /// <param name="account"></param>
        /// <returns>List of characters for the account</returns>
        public List<Character> ReadCharactersOfAccount(Account account)
        {
            Console.WriteLine("Reading characters...");

            List<Character> characters = new List<Character>();

            MySqlConnection conn = new MySqlConnection(ConnectionString);

            try
            {
                conn.Open();

                string command = "SELECT * FROM authory.character WHERE account_id=?accountId";
                MySqlCommand cmd = new MySqlCommand(command, conn);

                cmd.Parameters.AddWithValue("accountId", account.AccountId);

                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Character character = new Character()
                    {
                        Account = account,
                        CharacterId = reader.GetInt32(0),
                        Name = reader.GetString(2),
                        Level = (byte)reader.GetInt32(3),
                        ModelType = (byte)reader.GetInt32(4),
                        Experience = reader.GetInt32(5),
                        MapIndex = reader.GetInt32(6),
                        PositionX = reader.GetFloat(7),
                        PositionZ = reader.GetFloat(8),
                        Health = reader.GetInt32(9),
                        Mana = reader.GetInt32(10),
                    };

                    characters.Add(character);
                }
                Console.WriteLine("Reading characters done.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return characters;
        }

        /// <summary>
        /// Updates a character record in the database
        /// </summary>
        /// <param name="character"></param>
        /// <returns>Affected rows</returns>
        public int UpdateCharacter(Character character)
        {
            Console.WriteLine("Updating character...");
            Console.WriteLine("SAVING MAP INDEX: " + character.MapIndex);

            MySqlConnection conn = new MySqlConnection(ConnectionString);
            int affectedRows = 0;

            try
            {
                conn.Open();

                string command = "UPDATE authory.character SET experience=?experience, level=?level, map_index=?map_index, position_x=?pos_x, position_z=?pos_z, health=?health, mana=?mana " +
                    "WHERE id=?id AND account_id=?account_id";

                MySqlCommand cmd = new MySqlCommand(command, conn);

                cmd.Parameters.AddWithValue("experience", character.Experience);
                cmd.Parameters.AddWithValue("level", character.Level);
                cmd.Parameters.AddWithValue("map_index", character.MapIndex);
                cmd.Parameters.AddWithValue("pos_x", character.PositionX);
                cmd.Parameters.AddWithValue("pos_z", character.PositionZ);
                cmd.Parameters.AddWithValue("id", character.CharacterId);
                cmd.Parameters.AddWithValue("account_id", character.Account.AccountId);
                cmd.Parameters.AddWithValue("health", character.Health);
                cmd.Parameters.AddWithValue("mana", character.Mana);

                affectedRows = cmd.ExecuteNonQuery();

                Console.WriteLine($"Character updated.\nAffected rows: {affectedRows}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return affectedRows;
        }
    }
}