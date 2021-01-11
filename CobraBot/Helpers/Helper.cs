using Discord;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CobraBot.Helpers
{
    public static class Helper
    {
        //HttpClient field to use on our http requests (Instantiated only once)
        private static readonly HttpClient Client = new();


        /// <summary>Used to check if role exists. </summary>
        /// <returns>Returns an IRole if it exists, null if it doesn't.</returns>
        /// <param name="guild">Guild to run the check against.</param>
        /// <param name="roleName">The role to be checked if it exists.</param>
        public static IRole DoesRoleExist(IGuild guild, string roleName)
        {
            return guild.Roles.FirstOrDefault(role => role.Name.Contains(roleName));
        }

        /// <summary>Checks if specified string contains digits only. </summary>
        /// <returns>Returns 'true' if the string contains only digits, 'false' if it doesn't.</returns>
        /// <param name="str">The string to be checked.</param>
        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        /// <summary>Indents specified json string.</summary>
        /// <returns>Returns indented json.</returns>
        /// <param name="json">The json string to be indented.</param>
        public static string FormatJson(string json)
        {
            var parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        /// <summary>Makes the first letter of a string UPPERCASE. </summary>
        /// <returns>Returns specified string with it's first letter uppercase.</returns>
        /// <param name="str">The string to have it's first letter made uppercase.</param>
        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        /// <summary>Retrieve json response from specified http request. </summary>
        /// <returns>Returns HTTP response content.</returns>
        /// <param name="request">The RequestMessage to send.</param>
        public static async Task<string> HttpRequestAndReturnJson(HttpRequestMessage request)
        {
            string responseBody;

            try
            {
                //Try to send the request
                var response = await Client.SendAsync(request);

                //Make sure the request was successful
                response.EnsureSuccessStatusCode();

                //Save the request response to responseBody
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return await Task.FromException<string>(e);
            }

            //And if no errors occur, return the http response
            return await Task.FromResult(responseBody);
        }
    }
}
