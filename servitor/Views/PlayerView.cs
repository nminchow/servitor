using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static servitor.DestinyClient.SearchResultWrapper;

namespace servitor.Views
{
    partial class ViewBuilder
    {
        public Embed PlayerView(JObject player, string displayName)
        {
            var author = new EmbedAuthorBuilder();
            author.IconUrl = ViewHelpers.AsUrl((string)player["emblemPath"]);
            var platform = reversePlatformLookup[(string)player["membershipType"]];
            //author.Name = String.Format("{0} {1}", platformNameLookup[platform], displayName);
            author.Name = displayName;

            EmbedBuilder embed = new EmbedBuilder();
            embed.ThumbnailUrl = ViewHelpers.AsUrl((string)player["emblemPath"]);
            embed.Author = author;
            embed.AddInlineField("Light", player["light"]);
            var className = Manifest["DestinyClassDefinition"][(string)player["classHash"]]["displayProperties"]["name"];
            embed.AddInlineField("Class", className);
            return embed.Build();
        }
    }
}
