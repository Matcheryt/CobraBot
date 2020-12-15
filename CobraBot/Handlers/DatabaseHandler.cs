using System;
using System.Collections.Concurrent;
using System.Data.SQLite;

namespace CobraBot.Handlers
{   
    public static class DatabaseHandler
    {
        //Reference to database file
        private const string DatabaseLocation = "Data Source=database.db;journal_mode = wal;synchronous = 1";

        //Concurrent dictionary to store guild settings at runtime,
        //so we don't need to access the DB every time we need to check information
        private static ConcurrentDictionary<ulong, GuildSettings> _guildSettings = new ConcurrentDictionary<ulong, GuildSettings>();

        /// <summary>Initialize database and populate Concurrent Dictionary.
        /// </summary>
        public static void Initialize()
        {
            using var connection = new SQLiteConnection(DatabaseLocation);
            connection.Open();

            var command = connection.CreateCommand();

            //We check if the table 'prefixes' exists
            command.CommandText = "SELECT * FROM sqlite_master WHERE type='table' AND name = 'guildSettings';";
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

            //If table exists, we will select every column from guildSettings table
            command.CommandText = "SELECT guild, prefix, roleOnJoin, joinLeaveChannel FROM guildSettings";
            var dataReader = command.ExecuteReader();

            //While there is something to read from the database
            while (dataReader.Read())
            {
                //We will add the guild and the respective prefix to the prefixes dictionary
                _guildSettings.TryAdd(Convert.ToUInt64(CheckIfDbNull(dataReader["guild"])), new GuildSettings(CheckIfDbNull(dataReader["prefix"]), CheckIfDbNull(dataReader["roleOnJoin"]), CheckIfDbNull(dataReader["joinLeaveChannel"])));
            }

            dataReader.Close();
            connection.Close();
        }

        //Check if value is DBNull
        public static string CheckIfDbNull(object o)
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
            _guildSettings.TryGetValue(guildId, out GuildSettings settings);
            
            return settings ?? new GuildSettings(null, null, null);
        }

        /// <summary>Updates channel on the database.
        /// </summary>
        public static void UpdateChannelDb(ulong guildId, char operation, string channel = null)
        {
            using var connection = new SQLiteConnection(DatabaseLocation);
            connection.Open();

            var cmd = connection.CreateCommand();
            var currentSettings = RetrieveGuildSettings(guildId);

            switch (operation)
            {
                case '+':
                    cmd.CommandText = "INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES (@guildId, @prefix, @roleOnJoin, @channel);";
                    cmd.Parameters.AddWithValue("@guildId", guildId);
                    cmd.Parameters.AddWithValue("@prefix", currentSettings.Prefix);
                    cmd.Parameters.AddWithValue("@roleOnJoin", currentSettings.RoleOnJoin);
                    cmd.Parameters.AddWithValue("@channel", channel);
                    cmd.ExecuteNonQuery();

                    _guildSettings.AddOrUpdate(guildId, new GuildSettings(currentSettings.Prefix, currentSettings.RoleOnJoin, channel), (key, oldValue) => new GuildSettings(currentSettings.Prefix, currentSettings.RoleOnJoin, channel));
                    break;

                case '-':
                    cmd.CommandText = "INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES (@guildId, @prefix, @roleOnJoin, NULL)";
                    cmd.Parameters.AddWithValue("@guildId", guildId);
                    cmd.Parameters.AddWithValue("@prefix", currentSettings.Prefix);
                    cmd.Parameters.AddWithValue("@roleOnJoin", currentSettings.RoleOnJoin);
                    cmd.ExecuteNonQuery();

                    _guildSettings.AddOrUpdate(guildId, new GuildSettings(currentSettings.Prefix, currentSettings.RoleOnJoin, null), (key, oldValue) => new GuildSettings(currentSettings.Prefix, currentSettings.RoleOnJoin, null));
                    break;
            }

            connection.Close();
        }

        /// <summary>Updates prefix on the database.
        /// </summary>
        public static void UpdatePrefixDb(ulong guildId, char operation, string prefix = null)
        {
            using var connection = new SQLiteConnection(DatabaseLocation);
            connection.Open();

            var cmd = connection.CreateCommand();
            var currentSettings = RetrieveGuildSettings(guildId);

            switch (operation)
            {
                case '+':
                    cmd.CommandText = "INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES (@guildId, @prefix, @roleOnJoin, @channel);";
                    cmd.Parameters.AddWithValue("@guildId", guildId);
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    cmd.Parameters.AddWithValue("@roleOnJoin", currentSettings.RoleOnJoin);
                    cmd.Parameters.AddWithValue("@channel", currentSettings.JoinLeaveChannel);
                    cmd.ExecuteNonQuery();

                    _guildSettings.AddOrUpdate(guildId, new GuildSettings(prefix, currentSettings.RoleOnJoin, currentSettings.JoinLeaveChannel), (key, oldValue) => new GuildSettings(prefix, currentSettings.RoleOnJoin, currentSettings.JoinLeaveChannel));
                    break;

                case '-':
                    cmd.CommandText = "INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES (@guildId, NULL, @roleOnJoin, @channel)";
                    cmd.Parameters.AddWithValue("@guildId", guildId);
                    cmd.Parameters.AddWithValue("@roleOnJoin", currentSettings.RoleOnJoin);
                    cmd.Parameters.AddWithValue("@channel", currentSettings.JoinLeaveChannel);
                    cmd.ExecuteNonQuery();

                    _guildSettings.AddOrUpdate(guildId, new GuildSettings(null, currentSettings.RoleOnJoin, currentSettings.JoinLeaveChannel), (key, oldValue) => new GuildSettings(null, currentSettings.RoleOnJoin, currentSettings.JoinLeaveChannel));
                    break;
            }

            connection.Close();
        }

        /// <summary>Updates role on the database.
        /// </summary>
        public static void UpdateRoleOnJoinDB(ulong guildId, char operation, string roleName = null)
        {
            using var connection = new SQLiteConnection(DatabaseLocation);
            connection.Open();

            var cmd = connection.CreateCommand();
            var currentSettings = RetrieveGuildSettings(guildId);

            switch (operation)
            {
                case '+':
                    cmd.CommandText = "INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES (@guildId, @prefix, @roleOnJoin, @channel);";
                    cmd.Parameters.AddWithValue("@guildId", guildId);
                    cmd.Parameters.AddWithValue("@prefix", currentSettings.Prefix);
                    cmd.Parameters.AddWithValue("@roleOnJoin", roleName);
                    cmd.Parameters.AddWithValue("@channel", currentSettings.JoinLeaveChannel);
                    cmd.ExecuteNonQuery();

                    _guildSettings.AddOrUpdate(guildId, new GuildSettings(currentSettings.Prefix, roleName, currentSettings.JoinLeaveChannel), (key, oldValue) => new GuildSettings(currentSettings.Prefix, roleName, currentSettings.JoinLeaveChannel));
                    break;

                case '-':
                    cmd.CommandText = "INSERT OR REPLACE INTO guildSettings (guild, prefix, roleOnJoin, joinLeaveChannel) VALUES (@guildId, @prefix, NULL, @channel)";
                    cmd.Parameters.AddWithValue("@guildId", guildId);
                    cmd.Parameters.AddWithValue("@prefix", currentSettings.Prefix);
                    cmd.Parameters.AddWithValue("@channel", currentSettings.JoinLeaveChannel);
                    cmd.ExecuteNonQuery();

                    _guildSettings.AddOrUpdate(guildId, new GuildSettings(currentSettings.Prefix, null, currentSettings.JoinLeaveChannel), (key, oldValue) => new GuildSettings(currentSettings.Prefix, null, currentSettings.JoinLeaveChannel));
                    break;
            }

            connection.Close();
        }
    }
}
