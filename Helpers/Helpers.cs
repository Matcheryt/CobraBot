using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CobraBot.Helpers
{
    public class Helpers
    {
        //Not much to say here

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
            catch (Exception)
            {
                Console.WriteLine("An error ocurred while making an Http Request. Check logs for more info");
            }

            //And if no errors occur, return the http response
            return await Task.FromResult(httpResponse);
        }

    }
}
