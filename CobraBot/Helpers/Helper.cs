using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace CobraBot.Helpers
{
    public static class Helper
    {
        //HttpClient field to use on our http requests (Instantiated only once)
        private static readonly HttpClient Client = new HttpClient();


        /// <summary>Used to check if bot has higher hierarchy than specified user.
        /// <para>Returns true if bot has higher hierarchy, false if it doesn't.</para>
        /// </summary>
        public static bool BotHasHigherHierarchy(SocketGuildUser user, SocketCommandContext context)
        {
            //Returns true if bot has higher hierarchy
            return user.Hierarchy < context.Guild.CurrentUser.Hierarchy;
        }

        /// <summary>Used to check if role exists.
        /// <para>Returns an IRole if it exists, null if it doesn't.</para>
        /// </summary>
        public static IRole DoesRoleExist(IGuild guild, string roleName)
        {
            return guild.Roles.FirstOrDefault(role => role.Name.Contains(roleName));
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
