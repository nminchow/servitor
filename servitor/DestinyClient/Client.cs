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
    public class Client
    {
        static HttpClient httpClient = new HttpClient();

        public Client(IConfiguration config)
        {
            httpClient.BaseAddress = new Uri("https://www.bungie.net/Platform/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("X-API-Key", config["bungieApiKey"]);
        }

        private async Task<HttpResponseMessage> QueryApi(string path, string term)
        {
            return await httpClient.GetAsync(path + WebUtility.UrlEncode(term));
        }

        private async Task<JObject> SearchBungieUsers(string term)
        {
            HttpResponseMessage response = await QueryApi("User/SearchUsers/?q=", term);

            var result = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(result);
        }

        public async Task<string> SearchPvpData(string term)
        {
            var searchResult = await SearchBungieUsers(term);

            switch (searchResult["Response"].Count())
            {
                case 0:
                    return "User not found.";
                case 1:
                    return await SquadPvpView(DisplayNameFromResult(searchResult["Response"][0]));
                default:
                    {
                        return "candidates: " + String.Join(String.Empty, searchResult["Response"].Select(c => DisplayNameFromResult(c)).ToList());
                    }

            }
        }

        private string DisplayNameFromResult(JToken result)
        {
            return (string)(result["blizzardDisplayName"] ?? result["xboxDisplayName"] ?? result["psnDisplayName"]);
        }

        public async Task<string> SquadPvpView(string displayName)
        {
            var p = await SearchPlayer(displayName);
            return p.MembershipId;
        }

        private async Task<MembershipData> SearchPlayer(string term)
        {
            // TODO: loop over results and do case sensitive displayName == searchTerm check
            HttpResponseMessage response = await QueryApi("User/SearchUsers/?q=", term);
            var result = response.Content.ReadAsStringAsync().Result;
            var responseObject = JObject.Parse(result);

            return new MembershipData(
                (string)responseObject["Response"][0]["membershipType"],
                (string)responseObject["Response"][0]["membershipId"]);
        }

    }

    class MembershipData
    {
        public string MembershipType { get; set; }
        public string MembershipId { get; set; }

        public MembershipData(string type, string id)
        {
            MembershipType = type;
            MembershipId = id;
        }
    }
}
