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

using System.Drawing;
using Discord;
using System.Threading.Tasks;
using Victoria;
using Color = Discord.Color;

namespace CobraBot.Common.EmbedFormats
{
    public static class CustomFormats
    {
        /// <summary> Creates a basic embed with specified information and returns it. </summary>
        /// <returns> The created embed. </returns>
        public static Embed CreateBasicEmbed(string title, string description, uint rawValue)
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(rawValue).Build();
            return embed;
        }

        /// <summary> Creates a basic embed with specified information and returns it. </summary>
        /// <returns> The created embed. </returns>
        public static Embed CreateBasicEmbed(string title, string description, Color color)
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color).Build();
            return embed;
        }

        /// <summary> Creates an error embed with specified information and returns it. </summary>
        /// <returns> The created embed. </returns>
        public static Embed CreateErrorEmbed(string error)
        {
            var embed = new EmbedBuilder()
                .WithDescription($"{error}")
                .WithColor(Color.DarkRed).Build();
            return embed;
        }


        /// <summary> Creates a basic embed with specified information and returns it. </summary>
        /// <returns> The created embed. </returns>
        public static Embed CreateColorEmbed(string colorImage, int r, int g, int b, string hexColor)
        {
            var embed = new EmbedBuilder()
                .WithDescription($"**RGB:** {r}, {g}, {b}\n**Hex:** {hexColor}")
                .WithImageUrl(colorImage)
                .WithColor(r, g, b).Build();
            return embed;
        }


        /// <summary> Creates a now playing embed for specified LavaTrack and returns the embed. </summary>
        /// <returns> The created embed. </returns>
        public static async Task<Embed> NowPlayingEmbed(LavaTrack track, bool withDuration = false)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Now playing :cd:")
                .WithDescription($"[{track.Title}]({track.Url})")
                .WithThumbnailUrl(await track.FetchArtworkAsync())
                .WithColor(Color.Blue);

            if (withDuration)
                embed.WithDescription($"[{track.Title}]({track.Url})\n({track.Position:hh\\:mm\\:ss}/{track.Duration:hh\\:mm\\:ss})");

            return embed.Build();
        }

        /// <summary> Creates an information embed with specified information and returns it. </summary>
        /// <returns> The created embed. </returns>
        public static Embed CreateInfoEmbed(string title, string description, EmbedFooterBuilder footer, string iconUrl, EmbedFieldBuilder[] fields)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder().WithName(title).WithIconUrl(iconUrl))
                .WithFields(fields)
                .WithDescription(description)
                .WithColor(0x268618)
                .WithFooter(footer)
                .Build();
            return embed;
        }

    }
}
