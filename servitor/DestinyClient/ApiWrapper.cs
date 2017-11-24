using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static servitor.DestinyClient.SearchResultWrapper;

namespace servitor.DestinyClient
{
    public class ApiWrapper
    {

        static Client client;

        public ApiWrapper(Client c)
        {
            client = c;
        }

        public async Task<IEnumerable<Task<Discord.Embed>>> SearchPvpData(string term, string platform)
        {
            var searchResult = await client.SearchBungieUsers(term);
            var type = reversePlatformNameLookup[platform.ToLower()];

            switch (searchResult["Response"].Count())
            {
                case 0:
                    return new Task<Discord.Embed>[] { };
                default:
                    {
                        return searchResult["Response"]
                            .Select(p => new SearchResult((JObject)p, term))
                            .Where(p => p.type == type)
                            .OrderBy(p => p.identifier.Split('#').First().Length)
                            .Take(10)
                            .Select(async p => await SearchResultToView(p));
                    }

            }
        }

        public async Task<Discord.Embed> SquadPvpView(string displayName, string platform)
        {
            var type = reversePlatformNameLookup[platform.ToLower()];
            var p = await client.SearchPlayer(displayName, type);
            return await client.GetPlayerDetails(p.First());
        }

        private async Task<Discord.Embed> SearchResultToView(SearchResult result)
        {
            var items = await client.SearchPlayer(result.identifier, result.type);
            if (items.Any() == false)
            {
                return null;
            }
            return await client.GetPlayerDetails(items.First());
        }
    }
}
