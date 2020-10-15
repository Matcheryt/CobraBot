using Discord;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CobraBot.Helpers
{
    public class Helpers
    {
        //Not much to say here

        //Error builder
        public readonly EmbedBuilder errorBuilder = new EmbedBuilder().WithColor(Color.Red);

        //Verify if string contains only numbers
        public bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        //Uppercase first letter of a string
        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        //Method to request and return json from api calls
        public async Task<string> HttpRequestAndReturnJson(string request)
        {
            string httpResponse = null;

            try
            {
                //Create request to specified url
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(request);
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)(await httpWebRequest.GetResponseAsync()))
                {
                    //Process the response
                    using (Stream stream = httpWebResponse.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                        httpResponse += await reader.ReadToEndAsync();
                }
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    return await Task.FromResult("Not found");
                }
            }

            //And if no errors occur, return the http response
            return await Task.FromResult(httpResponse);
        }

    }
}
