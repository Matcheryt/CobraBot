using Discord;

namespace CobraBot.Common
{
    public static class EmbedFormats
    {
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

        /// <summary>Creates a basic embed with specified information and returns it.
        /// </summary>
        public static Embed CreateBasicEmbed(string title, string description, Color color)
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color).Build();
            return embed;
        }

        /// <summary>Creates an error embed with specified information and returns it.
        /// </summary>
        public static Embed CreateErrorEmbed(string error)
        {
            var embed = new EmbedBuilder()
                .WithDescription($"{error}")
                .WithColor(Color.DarkRed).Build();
            return embed;
        }

        /// <summary>Creates an embed with specified information and returns it.
        /// </summary>
        public static Embed CreateMusicEmbed(string title, string description, string thumbnailUrl)
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithThumbnailUrl(thumbnailUrl)
                .WithColor(Color.Blue).Build();
            return embed;
        }

        /// <summary>Creates an information embed with specified information and returns it.
        /// </summary>
        public static Embed CreateInfoEmbed(string title, EmbedFooterBuilder footer, string iconUrl, EmbedFieldBuilder[] fields)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder().WithName(title).WithIconUrl(iconUrl))
                .WithFields(fields)
                .WithColor(Color.DarkGreen)
                .WithFooter(footer)
                .Build();
            return embed;
        }

    }
}
