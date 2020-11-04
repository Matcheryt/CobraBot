using System;
using System.IO;
using Newtonsoft.Json.Linq;
using CobraBot.Helpers;

namespace CobraBot
{
    public static class Configuration
    {
        static string json;
        static JObject jsonParsed;

        //Constructor that checks if configuration file exists
        static Configuration()
        {
            StreamReader sr;

            //Try to read configuration file
            try
            {
                sr = new StreamReader("botconfig.json");
                json = sr.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }
            catch (FileNotFoundException)
            {
                //If the file is not found, then create the file with the needed parameters
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("|ERROR| botconfig.json configuration file not found, creating one...");
                StreamWriter sw = new StreamWriter("botconfig.json");
                string textToWrite = Helper.FormatJson("{ \"Tokens\": { \"Publish\": \"PUBLISH_TOKEN_HERE\", \"Develop\": \"DEVELOP_TOKEN_HERE\" }, \"APIKEYS\": { \"Steam\": \"API_KEY_HERE\", \"OWM\": \"API_KEY_HERE\", \"OxfordDictionary\": \"API_KEY_HERE\", \"OxfordAppId\":\"OXFORD_APP_ID_HERE\" } }");
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
        }

        /// <summary>Method used to retrieve data saved in botconfig.json file.
        /// </summary>
        public static string ReturnSavedValue(string obj, string prop)
        {
            var valueToRetrieve = jsonParsed[obj][prop];
            return (string)valueToRetrieve;
        }       

    }
}
