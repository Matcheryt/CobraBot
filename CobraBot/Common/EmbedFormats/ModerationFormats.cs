﻿using CobraBot.Database.Models;
using Discord;

namespace CobraBot.Common.EmbedFormats
{
    public static class ModerationFormats
    {
        /// <summary>Creates an embed to send to a user that has been punished.
        /// </summary>
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

        /// <summary>Creates a mod log embed according to specified mod case.
        /// </summary>
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
                    embed.WithTitle($"{modCase.UserName} banned | Case #{modCase.Id}");
                    break;

                case PunishmentType.Kick:
                    embed.WithTitle($"{modCase.UserName} kicked | Case #{modCase.Id}");
                    break;

                case PunishmentType.Mute:
                    embed.WithTitle($"{modCase.UserName} muted | Case #{modCase.Id}");
                    break;

                case PunishmentType.VMute:
                    embed.WithTitle($"{modCase.UserName} voice muted | Case #{modCase.Id}");
                    break;
            }

            return embed.Build();
        }

        /// <summary>Creates an embed with specified mod case information, used for a quick lookup about mod case info.
        /// </summary>
        public static Embed LookupEmbed(ModCase modCase, string username)
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

            var userNameField = new EmbedFieldBuilder().WithName("User Name").WithValue(username)
                .WithIsInline(true);
            var userIdField = new EmbedFieldBuilder().WithName("User ID").WithValue(modCase.UserId)
                .WithIsInline(true);
            var responsibleModField = new EmbedFieldBuilder().WithName("Responsible Mod").WithValue(modCase.ModName)
                .WithIsInline(true);
            var reasonField = new EmbedFieldBuilder().WithName("Reason").WithValue(modCase.Reason ?? "_No reason_")
                .WithIsInline(true);

            var embed = new EmbedBuilder()
                .WithTitle($"Case #{modCase.Id}")
                .WithFields(punishment, userNameField, userIdField, responsibleModField, reasonField)
                .WithColor(Color.LightGrey)
                .WithTimestamp(modCase.DateTime);

            return embed.Build();
        }


        /// <summary>Creates a moderation embed with specified information and returns it.
        /// </summary>
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