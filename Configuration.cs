using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;

namespace CobraBot
{
    public class Configuration
    {
        private string json;
        JObject jsonParsed;

        public Configuration()
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader("botconfig.json");
                json = sr.ReadToEnd();
                jsonParsed = JObject.Parse(json);
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("|WARNING| botconfig.json configuration file not found, creating one...");
                StreamWriter sw = new StreamWriter("botconfig.json");
                string textToWrite = FormatJson("{ \"Tokens\": { \"Publish\": \"PUBLISH_TOKEN_HERE\", \"Develop\": \"DEVELOP_TOKEN_HERE\" }, \"APIKEYS\": { \"Steam\": \"API_KEY_HERE\", \"OWM\": \"API_KEY_HERE\", \"OxfordDictionary\": \"API_KEY_HERE\", \"OxfordAppId\":\"OXFORD_APP_ID_HERE\" } }");
                sw.Write(textToWrite);
                sw.Flush();
                sw.Close();
                Console.WriteLine("|WARNING| The file botconfig.json has been created successfully\n|WARNING| Be sure to modify botconfig.json file accordingly");
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
                Environment.Exit(0);
            }            
        }

        public string ReturnSavedValue(string obj, string prop)
        {
            var valueToRetrieve = jsonParsed[obj][prop];
            return (string)valueToRetrieve;
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

    }
}
