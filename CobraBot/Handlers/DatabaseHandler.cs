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

        //Concurrent dictionary to store guild settings at runtime,
        //so we don't need to access the DB everytime we need to check information
        static ConcurrentDictionary<ulong, GuildSettings> guildSettings = new ConcurrentDictionary<ulong, GuildSettings>();

        //We can use a tuple on welcomeChannel dictionary to customize the message to be sent when someone joins/leaves
        //static Tuple<string, string> test;

        /// <summary>Initialize database and populate Concurrent Dictionary.
        /// </summary>
        public static void Initialize()
        {
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            var command = connection.CreateCommand();

            //We check if the table 'prefixes' exists
            command.CommandText = $"SELECT * FROM sqlite_master WHERE type='table' AND name = 'guildSettings';";
            var tableExists = command.ExecuteScalar();

            if (tableExists == null)
            {
                //Create a table called guildSettings where we will store each guild settings
                //NOTE: IDS are stored as text because sqlite doesn't support ulongs
                command.CommandText = "CREATE TABLE guildSettings (guild TEXT PRIMARY KEY, prefix TEXT, roleOnJoin TEXT, joinLeaveChannel TEXT);";
                command.ExecuteNonQuery();
                command.CommandText = "CREATE UNIQUE INDEX idx_guildSettings_id ON guildSettings (guild);";
                command.ExecuteNonQuery();
                return;
            }

            SQLiteDataReader dataReader;

            //If table exists, we will select every column from guildSettings table
            command.CommandText = "SELECT guild, prefix, roleOnJoin, joinLeaveChannel FROM guildSettings";
            dataReader = command.ExecuteReader();

            //While there is something to read from the database
            while (dataReader.Read())
            {
                //We will add the guild and the respective prefix to the prefixes dictionary
                guildSettings.TryAdd(Convert.ToUInt64(CheckIfDBNull(dataReader["guild"])), new GuildSettings(CheckIfDBNull(dataReader["prefix"]), CheckIfDBNull(dataReader["roleOnJoin"]), CheckIfDBNull(dataReader["joinLeaveChannel"])));
            }

            dataReader.Close();
            connection.Close();
        }

        //Check if value is DBNull
        public static string CheckIfDBNull(Object o)
        {
            if (o == DBNull.Value)
            {
                return null;
            }

            return (string)o;
        }

        /// <summary>Retrieve guild settings from dictionary.
        /// </summary>
        public static GuildSettings RetrieveGuildSettings(ulong guildId)
        {
            guildSettings.TryGetValue(guildId, out GuildSettings settings);
            
            if (settings == null)
                return new GuildSettings(null, null, null);

            return settings;
        }

        /// <summary>Updates channel on the database.
        /// </summary>
        public static void UpdateChannelDB(ulong guildId, char operation, string channel = null)
        {
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            var cmd = connection.CreateCommand();
            var currentSettings = RetrieveGuildSettings(guildId);

            if (operation == '+')
            {
                cmd.CommandText = $"INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES ('{guildId}', '{currentSettings.prefix}', '{currentSettings.roleOnJoin}', '{channel}');";
                cmd.ExecuteNonQuery();
                connection.Close();

                guildSettings.AddOrUpdate(guildId, new GuildSettings(currentSettings.prefix, currentSettings.roleOnJoin, channel), (key, oldValue) => new GuildSettings(currentSettings.prefix, currentSettings.roleOnJoin, channel));
            }

            if (operation == '-')
            {
                cmd.CommandText = $"INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES ('{guildId}', '{currentSettings.prefix}', '{currentSettings.roleOnJoin}', NULL)";
                cmd.ExecuteNonQuery();
                connection.Close();

                guildSettings.AddOrUpdate(guildId, new GuildSettings(currentSettings.prefix, currentSettings.roleOnJoin, null), (key, oldValue) => new GuildSettings(currentSettings.prefix, currentSettings.roleOnJoin, null));
            }

            connection.Close();
        }

        /// <summary>Updates prefix on the database.
        /// </summary>
        public static void UpdatePrefixDB(ulong guildId, char operation, string prefix = null)
        {
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            var cmd = connection.CreateCommand();
            var currentSettings = RetrieveGuildSettings(guildId);

            if (operation == '+')
            {
                cmd.CommandText = $"INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES ('{guildId}', '{prefix}', '{currentSettings.roleOnJoin}', '{currentSettings.joinLeaveChannel}');";
                cmd.ExecuteNonQuery();

                guildSettings.AddOrUpdate(guildId, new GuildSettings(prefix, currentSettings.roleOnJoin, currentSettings.joinLeaveChannel), (key, oldValue) => new GuildSettings(prefix, currentSettings.roleOnJoin, currentSettings.joinLeaveChannel));
            }

            if (operation == '-')
            {
                cmd.CommandText = $"INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES ('{guildId}', NULL, '{currentSettings.roleOnJoin}', '{currentSettings.joinLeaveChannel}')";
                cmd.ExecuteNonQuery();

                guildSettings.AddOrUpdate(guildId, new GuildSettings(null, currentSettings.roleOnJoin, currentSettings.joinLeaveChannel), (key, oldValue) => new GuildSettings(null, currentSettings.roleOnJoin, currentSettings.joinLeaveChannel));
            }

            connection.Close();
        }

        /// <summary>Updates role on the database.
        /// </summary>
        public static void UpdateRoleOnJoinDB(ulong guildId, char operation, string roleName = null)
        {
            using var connection = new SQLiteConnection(databaseLocation);
            connection.Open();

            var cmd = connection.CreateCommand();
            var currentSettings = RetrieveGuildSettings(guildId);

            if (operation == '+')
            {
                cmd.CommandText = $"INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES ('{guildId}', '{currentSettings.prefix}', '{roleName}', {currentSettings.joinLeaveChannel});";
                cmd.ExecuteNonQuery();

                guildSettings.AddOrUpdate(guildId, new GuildSettings(currentSettings.prefix, roleName, currentSettings.joinLeaveChannel), (key, oldValue) => new GuildSettings(currentSettings.prefix, roleName, currentSettings.joinLeaveChannel));
            }

            if (operation == '-')
            {
                cmd.CommandText = $"INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES ('{guildId}','{currentSettings.prefix}', NULL, {currentSettings.joinLeaveChannel})";
                cmd.ExecuteNonQuery();

                guildSettings.AddOrUpdate(guildId, new GuildSettings(currentSettings.prefix, null, currentSettings.joinLeaveChannel), (key, oldValue) => new GuildSettings(currentSettings.prefix, null, currentSettings.joinLeaveChannel));
            }

            connection.Close();
        }
    }
}
