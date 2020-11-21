using Discord;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CobraBot.Helpers
{
    public static class Helper
    {
        //Not much to say here
        //Class that has some useful reusable functions and fields

        /// <summary>Creates an embed with specified information and returns it.
        /// </summary>
        public static async Task<Embed> CreateBasicEmbed(string title, string description, Color color)
        {
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color).Build()));
            return embed;
        }

        /// <summary>Creates an error embed with specified information and returns it.
        /// </summary>
        public static async Task<Embed> CreateErrorEmbed(string error)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithDescription($"{error}")
                .WithColor(Color.DarkRed).Build());
            return embed;
        }

        /// <summary>Checks if specified string contains digits only.
        /// <para>Returns 'true' if the string contains only digits.</para>
        /// </summary>
        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        /// <summary>Formats specified json string, indents it and then returns it.
        /// </summary>
        public static string FormatJson(string json)
        {
            var parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        /// <summary>Makes the first letter of a string UPPERCASE.
        /// </summary>
        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        /// <summary>Retrieve json response from specified http request.
        /// <para>Used to easily make an http request and retrieve it's response.</para>
        /// </summary>
        public static async Task<string> HttpRequestAndReturnJson(HttpWebRequest request)
        {           
            string httpResponse = null;

            try
            {
                //Create request to specified url
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    //Process the response
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                        httpResponse += await reader.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                return await Task.FromException<string>(e);
            }
            //And if no errors occur, return the http response
            return await Task.FromResult(httpResponse);
        }
    }
}