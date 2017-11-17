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
    class PlayerView
    {
        JObject Player;
        string DisplayName;

        public PlayerView(JObject p, string dn)
        {
            Player = p;
            DisplayName = dn;
        }

        public Embed Render()
        {
            var author = new EmbedAuthorBuilder();
            author.IconUrl = ViewHelpers.AsUrl((string)Player["emblemPath"]);
            var platform = reversePlatformLookup[(string)Player["membershipType"]];
            author.Name = String.Format("{0} {1}", DisplayName, platformNameLookup[platform]);

            EmbedBuilder embed = new EmbedBuilder();
            embed.ThumbnailUrl = ViewHelpers.AsUrl((string)Player["emblemPath"]);
            embed.Author = author;
            embed.AddInlineField("Light", Player["light"]);
            embed.AddInlineField("Class", Player["classHash"]);
            return embed.Build();
        }
    }
}
