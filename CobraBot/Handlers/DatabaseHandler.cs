using System;
using System.Collections.Concurrent;
using System.Data.SQLite;

namespace CobraBot.Handlers
{
    public static class DatabaseHandler
    {
        //Reference to database file
        static string databaseLocation = "Data Source=database.db;journal_mode = wal;synchronous = 1";

        //Concurrent dictionary to store prefixes on runtime, so we don't
        //need to access the database everytime we need to check the prefix
        static ConcurrentDictionary<ulong, string> prefixes = new ConcurrentDictionary<ulong, string>();

        /// <summary>Initialize database and populate Concurrent Dictionary.
        /// </summary>
        public static void Initialize()
        {
            //Establish connection if database exists. If database doesn't exist, then create it and connect to it.
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            //Check if table prefixes exists
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM sqlite_master WHERE type='table' AND name = 'prefixes';";
            var result = command.ExecuteScalar();

            //If table prefixes doesn't exist
            if (result == null)
            {
                //Create a table called prefixes, with a guild and prefix column
                //NOTE: guild id is stored as TEXT because sqlite database doesn't support ulongs
                command.CommandText = "CREATE TABLE prefixes (guild TEXT PRIMARY KEY, prefix TEXT);";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE UNIQUE INDEX idx_prefixes_id ON prefixes (guild);";
                command.ExecuteNonQuery();

                connection.Close();

                return;
            }

            //If table exists, we will select every value from guild and prefix columns
            command.CommandText = "SELECT guild, prefix FROM prefixes";
            var reader = command.ExecuteReader();

            //While there is something to read from the database
            while (reader.Read())
            {
                //We will add the guild and the respective prefix to the prefixes dictionary
                prefixes.TryAdd(Convert.ToUInt64(reader["guild"]), (string)reader["prefix"]);
            }

            connection.Close();
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

    }
}
