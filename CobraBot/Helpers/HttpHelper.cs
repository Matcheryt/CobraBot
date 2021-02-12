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

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CobraBot.Helpers
{
    public static class HttpHelper
    {
        //HttpClient instance
        internal static HttpClient HttpClient = new();

        /// <summary>Retrieve json response from specified http request. </summary>
        /// <returns>Returns HTTP response content.</returns>
        /// <param name="request">The RequestMessage to send.</param>
        public static async Task<string> HttpRequestAndReturnJson(HttpRequestMessage request)
        {
            string responseBody;

            try
            {
                //Try to send the request
                var response = await HttpClient.SendAsync(request);

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
