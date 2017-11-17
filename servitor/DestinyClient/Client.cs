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
            HttpResponseMessage response = await QueryApi("User/SearchUsers/?q=", term);

            var result = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(result);
        }

        private async Task<List<MembershipData>> SearchPlayer(string term, PlatformType type = PlatformType.Other)
        {
            // TODO: loop over results and do case sensitive displayName == searchTerm check
            var queryType = platformLookup[type];
            HttpResponseMessage response = await QueryApi(String.Format("Destiny2/SearchDestinyPlayer/{0}/", queryType), term);
            var result = response.Content.ReadAsStringAsync().Result;
            var responseObject = JObject.Parse(result);

            return responseObject["Response"].Select(
                    p => new MembershipData(
                    (string)p["membershipType"],
                    (string)p["membershipId"],
                    (string)p["displayName"])
                ).ToList();
        }

        private async Task<Discord.Embed> GetPlayerDetails(MembershipData player)
        {
            // TODO: loop over results and check for last played character
            var queryType = platformLookup[player.MembershipType];
            HttpResponseMessage response = await QueryApi(String.Format("Destiny2/{0}/Profile/{1}{2}", queryType, player.MembershipId, "?components=200"));
            var result = response.Content.ReadAsStringAsync().Result;
            // whole plethora of shit that could go wrong in here
            try
            {
                var responseObject = JObject.Parse(result);
                var data = (JObject)responseObject["Response"]["characters"]["data"];
                var character = (JObject)data.Values<JToken>().FirstOrDefault().First();

                return new PlayerView(character, player.DisplayName).Render();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Task<Discord.Embed>>> SearchPvpData(string term)
        {
            var searchResult = await SearchBungieUsers(term);

            switch (searchResult["Response"].Count())
            {
                case 0:
                    // fix me
                    throw new Exception();
                default:
                    {
                        return searchResult["Response"].Select(
                           async p => await SearchResultToView((JObject)p, term)).ToList();
                    }

            }
        }

        public async Task<Discord.Embed> SquadPvpView(string displayName, string platform)
        {
            var type = (PlatformType)Enum.Parse(typeof(PlatformType), platform);
            var p = await SearchPlayer(displayName, type);
            return await GetPlayerDetails(p.First());
        }

        private async Task<Discord.Embed> SearchResultToView(JObject p, string term)
        {
            var result = new SearchResult(p, term);
            if(result.type == PlatformType.Other)
            {
                return null;
            }
            var items = await SearchPlayer(result.identifier, result.type);
            return await GetPlayerDetails(items.First());
        }

    }
}
