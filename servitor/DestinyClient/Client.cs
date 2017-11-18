using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using servitor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static servitor.DestinyClient.SearchResultWrapper;

namespace servitor.DestinyClient
{
    public partial class Client
    {
        static HttpClient httpClient = new HttpClient();

        public static string BungieRootPath = "https://www.bungie.net";

        public Client(IConfiguration config)
        {
            httpClient.BaseAddress = new Uri(BungieRootPath + "/Platform/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("X-API-Key", config["bungieApiKey"]);
        }

        private async Task<HttpResponseMessage> QueryApi(string path, string term = "")
        {
            return await httpClient.GetAsync(path + WebUtility.UrlEncode(term));
        }

        private async Task<JObject> SearchBungieUsers(string term)
        {
            Console.WriteLine("Searching bungie users");
            HttpResponseMessage response = await QueryApi("User/SearchUsers/?q=", term);

            var result = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(result);
        }

        private async Task<IEnumerable<MembershipData>> SearchPlayer(string term, PlatformType type = PlatformType.Other)
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

        private async Task<Discord.Embed> GetPlayerDetails(MembershipData player)
        {
            Console.WriteLine("getting player details: " + player.DisplayName);
            // TODO: loop over results and check for last played character
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
            var character = (JObject)data.Values<JToken>().FirstOrDefault().First();
            Console.WriteLine("rendering view: " + player.DisplayName);

            return new PlayerView(character, player.DisplayName).Render();

        }

        public async Task<IEnumerable<Task<Discord.Embed>>> SearchPvpData(string term)
        {
            var searchResult = await SearchBungieUsers(term);

            switch (searchResult["Response"].Count())
            {
                case 0:
                    // fix me
                    throw new Exception();
                default:
                    {
                        return searchResult["Response"]
                            .Select(p => new SearchResult((JObject)p, term))
                            .Where(p => p.type != PlatformType.Other)
                            .OrderBy(p => p.identifier.Split('#').First().Length)
                            .Take(10)
                            .Select(async p => await SearchResultToView(p));
                    }

            }
        }

        public async Task<Discord.Embed> SquadPvpView(string displayName, string platform)
        {
            var type = reversePlatformNameLookup[platform];
            var p = await SearchPlayer(displayName, type);
            return await GetPlayerDetails(p.First());
        }

        private async Task<Discord.Embed> SearchResultToView(SearchResult result)
        {
            var items = await SearchPlayer(result.identifier, result.type);
            return await GetPlayerDetails(items.First());
        }

    }
}
