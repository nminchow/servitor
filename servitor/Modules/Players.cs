using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace servitor.Modules
{
    // Create a module with no prefix
    [Group("serv")]
    public class Players : ModuleBase<SocketCommandContext>
    {
        private DestinyClient.Client _client;

        public Players(DestinyClient.Client client)
        {
            _client = client;
        }

        // ~say hello -> hello
        [Command("s", RunMode = RunMode.Async)]
        [Summary("Echos a message.")]
        public async Task SearchPlayer([Remainder] [Summary("player name to query")] string name)
        {
            var result = await _client.SearchPvpData(name);
            var results = new List<Embed>();
            result.ForEach(p => AddIfNotNull(results, p.Result));

            // ReplyAsync is a method on ModuleBase
            results.ForEach(async p => await ReplyAsync(message: "", embed: p));
        }

        private void AddIfNotNull<T>(List<T> list, T item)
        {
            if( item != null)
            {
                list.Add(item);
            }
        }

        // ~say hello -> hello
        [Command("l")]
        [Summary("Echos a message.")]
        public async Task LookupPvpView([Summary("display name to look for")] string name, string platform)
        {
            Embed result = await _client.SquadPvpView(name, platform);

            // ReplyAsync is a method on ModuleBase
            await ReplyAsync(message: "" , embed: result);
        }
    }

    // Create a module with the 'sample' prefix
    [Group("sample")]
    public class Sample : ModuleBase<SocketCommandContext>
    {
        // ~sample square 20 -> 400
        [Command("square")]
        [Summary("Squares a number.")]
        public async Task SquareAsync([Summary("The number to square.")] int num)
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }

        // ~sample userinfo --> foxbot#0282
        // ~sample userinfo @Khionu --> Khionu#8708
        // ~sample userinfo Khionu#8708 --> Khionu#8708
        // ~sample userinfo Khionu --> Khionu#8708
        // ~sample userinfo 96642168176807936 --> Khionu#8708
        // ~sample whois 96642168176807936 --> Khionu#8708
        [Command("userinfo")]
        [Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync([Summary("The (optional) user to get info for")] SocketUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
        }
    }
}
