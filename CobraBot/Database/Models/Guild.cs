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

using System.ComponentModel.DataAnnotations;

namespace CobraBot.Database.Models
{
    public class Guild
    {
        [Key]
        public int Id { get; set; }

        public ulong GuildId { get; set; }
        public ulong WelcomeChannel { get; set; }
        public ulong ModerationChannel { get; set; }
        public string CustomPrefix { get; set; }
        public string RoleOnJoin { get; set; }
    }
}
