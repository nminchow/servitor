using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace servitor.Modules
{
    // Create a module with no prefix
    [Group("serv")]
    public class Players : ModuleBase<SocketCommandContext>
    {
        private DestinyClient.ApiWrapper destinyApi;

        public Players(DestinyClient.ApiWrapper api)
        {
            destinyApi = api;
        }

        // ~say hello -> hello
        [Command("s", RunMode = RunMode.Async)]
        [Summary("Echos a message.")]
        public async Task SearchPlayer([Summary("platform to search on")] string platform, [Remainder] [Summary("player name to query")] string name)
        {
            var result = await destinyApi.SearchPvpData(name, platform);
            var results = new List<Task<IUserMessage>>();
            results.Add(ReplyAsync("Searching..."));
            foreach (var item in result)
            {
                results.Add(SendEmbed(item));
            }

            await Task.WhenAll(results);
            await ReplyAsync("Search Complete.");
        }

        private async Task<IUserMessage> SendEmbed(Task<Embed> item)
        {
            var result = await item;
            if (result != null)
                return await ReplyAsync(message: "", embed: result);
            return null;
        }

        // ~say hello -> hello
        [Command("l")]
        [Summary("Echos a message.")]
        public async Task LookupPvpView([Summary("platform to search on")] string platform, [Remainder] [Summary("display name to look for")] string name)
        {
            Embed result = await destinyApi.SquadPvpView(name, platform);

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
