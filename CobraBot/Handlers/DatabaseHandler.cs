using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CobraBot.Handlers
{
    public static class DatabaseHandler
    {
        //Reference to database file
        static string databaseLocation = "Data Source=database.db;journal_mode = wal;synchronous = 1";

        //Concurrent dictionaries to store prefixes and welcome channels on runtime, so we don't
        //need to access the database everytime we need to check that information
        static ConcurrentDictionary<ulong, string> prefixes = new ConcurrentDictionary<ulong, string>();
        static ConcurrentDictionary<ulong, string> welcomeChannel = new ConcurrentDictionary<ulong, string>();

        //We can use a tuple on welcomeChannel dictionary to customize the message to be sent when someone joins/leaves
        //static Tuple<string, string> test;

        /// <summary>Initialize database and populate Concurrent Dictionary.
        /// </summary>
        public static void Initialize()
        {
            //If tables don't exist, then we return, as this initialize method
            //Serves to populate the Concurrent Dictionaries with database info            
            if (!CheckIfTablesExistAndSetup())
                return;

            //Establish connection if database exists. If database doesn't exist, then create it and connect to it.
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            //Check if table prefixes exists
            var command = connection.CreateCommand();

            SQLiteDataReader dataReader;

            //If table exists, we will select every value from guild and prefix columns
            command.CommandText = "SELECT guild, prefix FROM prefixes";
            dataReader = command.ExecuteReader();

            //While there is something to read from the database
            while (dataReader.Read())
            {
                //We will add the guild and the respective prefix to the prefixes dictionary
                prefixes.TryAdd(Convert.ToUInt64(dataReader["guild"]), (string)dataReader["prefix"]);
            }

            dataReader.Close();

            //If table exists, we will select every value from guild and prefix columns
            command.CommandText = "SELECT guild, welcomeChannel FROM welcome";
            dataReader = command.ExecuteReader();

            //While there is something to read from the database
            while (dataReader.Read())
            {
                //We will add the guild and the respective prefix to the prefixes dictionary
                welcomeChannel.TryAdd(Convert.ToUInt64(dataReader["guild"]), (string)dataReader["welcomeChannel"]);
            }

            dataReader.Close();

            connection.Close();
        }

        /// <summary>Check if necessary sqlite tables exist and create those tables if they don't exist.
        /// <para>Returns true if tables exist, and false if they don't exist</para>
        /// </summary>
        static bool CheckIfTablesExistAndSetup()
        {
            //Establish connection if database exists. If database doesn't exist, then create it and connect to it.
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            bool welcomeExists = true;
            bool prefixesExists = true;

            //Check if table prefixes exists
            var command = connection.CreateCommand();

            //We check if the table 'prefixes' exists
            command.CommandText = $"SELECT * FROM sqlite_master WHERE type='table' AND name = 'prefixes';";
            var prefixResult = command.ExecuteScalar();

            //We check if the table 'welcome' exists
            command.CommandText = $"SELECT * FROM sqlite_master WHERE type='table' AND name = 'welcome';";
            var welcomeResult = command.ExecuteScalar();

            //Set boolean values according to results from database query
            if (prefixResult == null)
                prefixesExists = false;

            if (welcomeResult == null)
                welcomeExists = false;

            //If both tables exist, then we return true, so the Initialize method knows
            //That it needs to populate the dictionaries with the database information
            if (welcomeExists && prefixesExists)
                return true;

            if (!welcomeExists)
            {
                //Create a table called welcome, with a guild and welcomeChannel columns
                //NOTE: welcomeChannel id is stored as TEXT because sqlite database doesn't support ulongs
                command.CommandText = "CREATE TABLE welcome (guild TEXT PRIMARY KEY, welcomeChannel TEXT);";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE UNIQUE INDEX idx_welcome_id ON welcome (guild);";
                command.ExecuteNonQuery();
            }

            if (!prefixesExists)
            {
                //Create a table called prefixes, with a guild and prefix columns
                //NOTE: guild id is stored as TEXT because sqlite database doesn't support ulongs
                command.CommandText = "CREATE TABLE prefixes (guild TEXT PRIMARY KEY, prefix TEXT);";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE UNIQUE INDEX idx_prefixes_id ON prefixes (guild);";
                command.ExecuteNonQuery();
            }

            connection.Close();
            return false;
        }

        /// <summary>Add guild/prefix pair to the database.
        /// </summary>
        public static void AddPrefixToDB(ulong guildId, string prefix)
        {
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = $"INSERT OR REPLACE INTO prefixes (guild, prefix) VALUES ('{guildId}', '{prefix}');";
            cmd.ExecuteNonQuery();
            connection.Close();

            prefixes.AddOrUpdate(guildId, prefix, (key, oldValue) => prefix);
        }

        /// <summary>Remove guild/prefix pair from the database.
        /// </summary>
        public static void RemovePrefixFromDB(ulong guildId)
        {
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = $"DELETE FROM prefixes WHERE guild = {guildId}";
            cmd.ExecuteNonQuery();
            connection.Close();

            prefixes.TryRemove(guildId, out _);
        }

        /// <summary>Get the prefix respective to the guildId specified from the Concurrent Dictionary.
        /// </summary>
        public static string GetPrefix(ulong guildId)
        {
            prefixes.TryGetValue(guildId, out string prefix);

            return prefix;
        }

        /// <summary>Add guild/prefix pair to the database.
        /// </summary>
        public static void AddChannelToDB(ulong guildId, string channel)
        {
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = $"INSERT OR REPLACE INTO welcome (guild, welcomeChannel) VALUES ('{guildId}', '{channel}');";
            cmd.ExecuteNonQuery();
            connection.Close();

            welcomeChannel.AddOrUpdate(guildId, channel, (key, oldValue) => channel);
        }

        /// <summary>Remove guild/prefix pair from the database.
        /// </summary>
        public static void RemoveChannelFromDB(ulong guildId)
        {
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText = $"DELETE FROM welcome WHERE guild = {guildId}";
            cmd.ExecuteNonQuery();
            connection.Close();

            welcomeChannel.TryRemove(guildId, out _);
        }

        /// <summary>Get the prefix respective to the guildId specified from the Concurrent Dictionary.
        /// </summary>
        public static string GetChannel(ulong guildId)
        {
            welcomeChannel.TryGetValue(guildId, out string channel);

            return channel;
        }

    }
}
