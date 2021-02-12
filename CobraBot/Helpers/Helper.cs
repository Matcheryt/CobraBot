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

using Discord;
using Newtonsoft.Json;
using System.Linq;

namespace CobraBot.Helpers
{
    public static class Helper
    {
        /// <summary>Used to check if role exists. </summary>
        /// <returns>Returns an IRole if it exists, null if it doesn't.</returns>
        /// <param name="guild">Guild to run the check against.</param>
        /// <param name="roleId">The role to be checked if it exists.</param>
        public static IRole DoesRoleExist(IGuild guild, ulong roleId)
        {
            return guild.Roles.FirstOrDefault(role => role.Id.Equals(roleId));
        }


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
    }
}
