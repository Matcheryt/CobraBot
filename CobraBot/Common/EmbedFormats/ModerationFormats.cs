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

using CobraBot.Database.Models;
using Discord;

namespace CobraBot.Common.EmbedFormats
{
    public static class ModerationFormats
    {
        /// <summary> Creates an embed used to send to a user that has been punished. </summary>
        /// <returns> The created embed. </returns>
        public static Embed DmPunishmentEmbed(string title, string description, IGuild guild)
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithFooter(x =>
                {
                    x.IconUrl = guild.IconUrl;
                    x.Text = guild.Name;
                })
                .WithColor(Color.DarkerGrey).Build();
            return embed;
        }

        /// <summary> Creates an embed to send to the moderation log channel when someone is unbanned. </summary>
        /// <returns> The created embed. </returns>
        public static Embed UnbanEmbed(IUser user, IUser responsibleMod = null)
        {
            var userNameField = new EmbedFieldBuilder().WithName("User Name").WithValue(user)
                .WithIsInline(true);
            var userIdField = new EmbedFieldBuilder().WithName("User ID").WithValue(user.Id)
                .WithIsInline(true);

            var embed = new EmbedBuilder()
                .WithTitle($"{user} unbanned")
                .WithFields(userNameField, userIdField);

            if (responsibleMod != null)
                embed.WithAuthor(responsibleMod);

            return embed.Build();
        }

        /// <summary> Creates a mod log embed according to specified mod case. </summary>
        /// <returns> The created embed. </returns>
        public static Embed ModLogEmbed(ModCase modCase)
        {
            //Fields
            var userNameField = new EmbedFieldBuilder().WithName("User Name").WithValue(modCase.UserName)
                .WithIsInline(true);
            var userIdField = new EmbedFieldBuilder().WithName("User ID").WithValue(modCase.UserId)
                .WithIsInline(true);
            var responsibleModField = new EmbedFieldBuilder().WithName("Responsible Mod").WithValue(modCase.ModName)
                .WithIsInline(true);
            var reasonField = new EmbedFieldBuilder().WithName("Reason").WithValue(modCase.Reason ?? "_No reason_")
                .WithIsInline(true);

            var embed = new EmbedBuilder()
                .WithTimestamp(modCase.DateTime)
                .WithColor(Color.DarkPurple)
                .WithFields(userNameField, userIdField, responsibleModField, reasonField);

            //Change title according to punishment type
            switch (modCase.PunishmentType)
            {
                case PunishmentType.Ban:
                    embed.WithTitle($"{modCase.UserName} banned | Case #{modCase.ModCaseId}");
                    break;

                case PunishmentType.Kick:
                    embed.WithTitle($"{modCase.UserName} kicked | Case #{modCase.ModCaseId}");
                    break;

                case PunishmentType.Mute:
                    embed.WithTitle($"{modCase.UserName} muted | Case #{modCase.ModCaseId}");
                    break;

                case PunishmentType.VMute:
                    embed.WithTitle($"{modCase.UserName} voice muted | Case #{modCase.ModCaseId}");
                    break;
            }

            return embed.Build();
        }

        /// <summary> Creates an embed with specified mod case information, used for a quick lookup about mod case info. </summary>
        /// <returns> The created embed. </returns>
        public static Embed LookupEmbed(ModCase modCase, string user, string mod)
        {
            var punishment = new EmbedFieldBuilder().WithName("Punishment Type");

            switch (modCase.PunishmentType)
            {
                case PunishmentType.Mute:
                    punishment.WithValue("Mute");
                    break;

                case PunishmentType.VMute:
                    punishment.WithValue("Voice Mute");
                    break;

                case PunishmentType.Kick:
                    punishment.WithValue("Kick");
                    break;

                case PunishmentType.Ban:
                    punishment.WithValue("Ban");
                    break;
            }

            var userNameField = new EmbedFieldBuilder().WithName("User Name").WithValue(user)
                .WithIsInline(true);
            var userIdField = new EmbedFieldBuilder().WithName("User ID").WithValue(modCase.UserId)
                .WithIsInline(true);
            var responsibleModField = new EmbedFieldBuilder().WithName("Responsible Mod").WithValue(mod)
                .WithIsInline(true);
            var reasonField = new EmbedFieldBuilder().WithName("Reason").WithValue(modCase.Reason ?? "_No reason_")
                .WithIsInline(true);

            var embed = new EmbedBuilder()
                .WithTitle($"Case #{modCase.ModCaseId}")
                .WithFields(punishment, userNameField, userIdField, responsibleModField, reasonField)
                .WithColor(Color.LightGrey)
                .WithTimestamp(modCase.DateTime);

            return embed.Build();
        }


        /// <summary> Creates a moderation embed with specified information. </summary>
        /// <returns> The created embed. </returns>
        public static Embed CreateModerationEmbed(IUser user, string title, string description, Color color)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder().WithIconUrl(user.GetAvatarUrl()).WithName(title))
                .WithDescription(description)
                .WithColor(color).Build();
            return embed;
        }
    }
}