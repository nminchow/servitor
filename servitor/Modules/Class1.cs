﻿using Discord.Commands;
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
    public class Info : ModuleBase<SocketCommandContext>
    {
        private DestinyClient.Client _client;

        public Info(DestinyClient.Client client)
        {
            _client = client;
        }

        // ~say hello -> hello
        [Command("s")]
        [Summary("Echos a message.")]
        public async Task SearchPlayer([Remainder] [Summary("player name to query")] string name)
        {
            string result = await _client.SearchPvpData(name);

            // ReplyAsync is a method on ModuleBase
            await ReplyAsync(result);
        }

        // ~say hello -> hello
        [Command("se")]
        [Summary("Echos a message.")]
        public async Task SearchPvpView([Remainder] [Summary("display name to look for")] string name)
        {
            string result = await _client.SquadPvpView(name);

            // ReplyAsync is a method on ModuleBase
            await ReplyAsync(result);
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