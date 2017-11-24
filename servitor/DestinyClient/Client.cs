using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using servitor.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static servitor.DestinyClient.SearchResultWrapper;

namespace servitor.DestinyClient
{
    public class Client
    {
        static HttpClient httpClient = new HttpClient();

        static ViewBuilder viewBuilder;

        public static string BungieRootPath = "https://www.bungie.net";

        public Client(IConfiguration config)
        {
            httpClient.BaseAddress = new Uri(BungieRootPath + "/Platform/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("X-API-Key", config["bungieApiKey"]);

            var text = File.ReadAllText(@"manifest.json");

            viewBuilder = new ViewBuilder(JObject.Parse(text));
        }

        private async Task<HttpResponseMessage> QueryApi(string path, string term = "")
        {
            return await httpClient.GetAsync(path + Uri.EscapeUriString(term));
        }

        public async Task<JObject> SearchBungieUsers(string term)
        {
            Console.WriteLine("Searching bungie users");
            HttpResponseMessage response = await QueryApi("User/SearchUsers/?q=", term);

            var result = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(result);
        }

        public async Task<IEnumerable<MembershipData>> SearchPlayer(string term, PlatformType type = PlatformType.Other)
        {
            Console.WriteLine("SearchingPlayer: " + term);
            // TODO: loop over results and do case sensitive displayName == searchTerm check
            var queryType = platformLookup[type];
            HttpResponseMessage response = await QueryApi(String.Format("Destiny2/SearchDestinyPlayer/{0}/", queryType), term);
            Console.WriteLine("done searching player: " + term);

            var result = response.Content.ReadAsStringAsync().Result;
            var responseObject = JObject.Parse(result);

            return responseObject["Response"].Select(
                    p => new MembershipData(
                    (string)p["membershipType"],
                    (string)p["membershipId"],
                    (string)p["displayName"])
                );
        }

        public async Task<Discord.Embed> GetPlayerDetails(MembershipData player)
        {
            Console.WriteLine("getting player details: " + player.DisplayName);
            var queryType = platformLookup[player.MembershipType];
            HttpResponseMessage response = await QueryApi(String.Format("Destiny2/{0}/Profile/{1}{2}", queryType, player.MembershipId, "?components=200"));
            Console.WriteLine("done player details: " + player.DisplayName);

            var result = response.Content.ReadAsStringAsync().Result;
            var responseObject = JObject.Parse(result);

            if ((int)responseObject["ErrorCode"] != 1)
            {
                return null;
            }
 
            var data = (JObject)responseObject["Response"]["characters"]["data"];
            var character = (JObject)data.Values<JToken>()
                .Select(p => p.FirstOrDefault())
                .OrderByDescending(p => DateTime.Parse((string)p["dateLastPlayed"])).First();
            Console.WriteLine("rendering view: " + player.DisplayName);

            return viewBuilder.PlayerView(character, player.DisplayName);
        }

    }
}
