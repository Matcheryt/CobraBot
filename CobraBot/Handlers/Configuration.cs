/*
    Multi-purpose Discord Bot named Cobra
    Copyright (C) 2021 Telmo Duarte <contact@telmoduarte.me>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>. 
*/

using CobraBot.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace CobraBot.Handlers
{
    public static class Configuration
    {
        private static readonly JObject JsonParsed;

        //Tokens and API Keys
        public static readonly string PublishToken;
        public static readonly string DevelopToken;
        public static readonly string DictApiKey;
        public static readonly string DictAppId;
        public static readonly string SteamDevKey;
        public static readonly string OwmApiKey;
        public static readonly string KSoftApiKey;
        public static readonly string OmdbApiKey;
        public static readonly string SentryApiKey;
        public static readonly string DblApiKey;
        public static readonly string TopggApiKey;

        //Constructor that checks if configuration file exists
        static Configuration()
        {
            //Try to read configuration file
            try
            {
                var sr = new StreamReader("botconfig.json");
                var json = sr.ReadToEnd();
                JsonParsed = JObject.Parse(json);
            }
            catch (FileNotFoundException)
            {
                //If the file is not found, then create the file with the needed parameters
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("|ERROR| botconfig.json configuration file not found, creating one...");
                var sw = new StreamWriter("botconfig.json");
                var textToWrite = Helper.FormatJson("{ \"Tokens\": { \"Publish\": \"PUBLISH_TOKEN_HERE\", \"Develop\": \"DEVELOP_TOKEN_HERE\" }, \"APIKEYS\": { \"Steam\": \"API_KEY_HERE\", \"OWM\": \"API_KEY_HERE\", \"OxfordDictionary\": \"API_KEY_HERE\", \"OxfordAppId\": \"APP_ID_HERE\", \"KSoft\": \"API_KEY_HERE\", \"OMDB\": \"API_KEY_HERE\", \"Sentry\": \"API_KEY_HERE\", \"DBL\": \"API_KEY_HERE\", \"TOPGG\": \"API_KEY_HERE\" } }");
                sw.Write(textToWrite);
                sw.Flush();
                sw.Close();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("|SUCCESS| The file botconfig.json has been created successfully\n|SUCCESS| Be sure to modify botconfig.json file accordingly");
                Console.ResetColor();
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            //If configuration file exists, we save the values from the configuration file into variables for access within the program
            PublishToken = ReturnSavedValue("Tokens", "Publish");
            DevelopToken = ReturnSavedValue("Tokens", "Develop");
            DictApiKey = ReturnSavedValue("APIKEYS", "OxfordDictionary");
            DictAppId = ReturnSavedValue("APIKEYS", "OxfordAppId");
            SteamDevKey = ReturnSavedValue("APIKEYS", "Steam");
            OwmApiKey = ReturnSavedValue("APIKEYS", "OWM");
            KSoftApiKey = ReturnSavedValue("APIKEYS", "KSoft");
            OmdbApiKey = ReturnSavedValue("APIKEYS", "OMDB");
            SentryApiKey = ReturnSavedValue("APIKEYS", "Sentry");
            DblApiKey = ReturnSavedValue("APIKEYS", "DBL");
            TopggApiKey = ReturnSavedValue("APIKEYS", "TOPGG");
        }

        /// <summary> Method used to retrieve data saved in botconfig.json file. </summary>
        private static string ReturnSavedValue(string obj, string prop)
        {
            var valueToRetrieve = JsonParsed[obj][prop];
            return (string)valueToRetrieve;
        }

    }
}
