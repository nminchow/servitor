using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace servitor.DestinyClient
{
    public partial class Client
    {
        static HttpClient httpClient = new HttpClient();

        public Client(IConfiguration config)
        {
            httpClient.BaseAddress = new Uri("https://www.bungie.net/Platform/");
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

        private async Task<MembershipData> SearchPlayer(string term, PlatformType type = PlatformType.Other)
        {
            // TODO: loop over results and do case sensitive displayName == searchTerm check
            var queryType = platformLookup[type];
            HttpResponseMessage response = await QueryApi(String.Format("Destiny2/SearchDestinyPlayer/{0}/", queryType), term);
            var result = response.Content.ReadAsStringAsync().Result;
            var responseObject = JObject.Parse(result);

            return new MembershipData(
                (string)responseObject["Response"][0]["membershipType"],
                (string)responseObject["Response"][0]["membershipId"]);
        }

        private async Task<JObject> GetPlayerDetails(MembershipData player)
        {
            // TODO: loop over results and check for last played character
            var queryType = platformLookup[player.MembershipType];
            HttpResponseMessage response = await QueryApi(String.Format("Destiny2/{0}/Profile/{1}{2}", queryType, player.MembershipId, "?components=200"));
            var result = response.Content.ReadAsStringAsync().Result;
            var responseObject = JObject.Parse(result);
            return responseObject;
        }

        public async Task<string> SearchPvpData(string term)
        {
            var searchResult = await SearchBungieUsers(term);

            switch (searchResult["Response"].Count())
            {
                case 0:
                    return "User not found.";
                case 1:
                    // only one user found, cut right to pvp breakdown
                    //return await SquadPvpView(SearchResultFromResult(searchResult["Response"][0]));
                    var result = new SearchResult(searchResult["Response"][0]);
                    var player = SearchPlayer(result.identifier, result.type);
                    return "bleh";
                default:
                    {
                        return "candidates: " + String.Join(String.Empty, searchResult["Response"].Select(c => SearchResultFromResult(c)).ToList());
                    }

            }
        }

        private string SearchResultFromResult(JToken result)
        {
            return (string)(result["blizzardDisplayName"] ?? result["xboxDisplayName"] ?? result["psnDisplayName"]);
        }

        public async Task<string> SquadPvpView(string displayName)
        {
            var p = await SearchPlayer(displayName);
            var player = await GetPlayerDetails(p);
            return "done";
        }

 

    }
}
