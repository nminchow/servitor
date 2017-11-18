using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace servitor.DestinyClient
{
    public class SearchResultWrapper
    {

        // could use manifest for these
        public static Dictionary<PlatformType, string> platformLookup = new Dictionary<PlatformType, string>
        {
            {PlatformType.PC, "4"},
            {PlatformType.Xbox, "1"},
            {PlatformType.PSN, "2"},
            {PlatformType.Other, "-1"}
        };

        public static Dictionary<string, PlatformType> reversePlatformLookup = platformLookup.ToDictionary(x => x.Value, x => x.Key);

        // could use manifest for these
        public static Dictionary<PlatformType, string> platformNameLookup = new Dictionary<PlatformType, string>
        {
            {PlatformType.PC, "PC"},
            {PlatformType.Xbox, "Xbox"},
            {PlatformType.PSN, "PSN"},
            {PlatformType.Other, "Other"}
        };

        public static Dictionary<string, PlatformType> reversePlatformNameLookup = platformNameLookup.ToDictionary(x => x.Value, x => x.Key);



        public enum PlatformType
        {
            PC,
            PSN,
            Xbox,
            Other
        }

        public struct SearchResult
        {
            public string identifier;
            public PlatformType type;

            public SearchResult(JToken queryResult, string term)
            {
                var blizz = queryResult["blizzardDisplayName"];
                var xbox = queryResult["xboxDisplayName"];
                var psn = queryResult["psnDisplayName"];
                if (Check(blizz, term))
                {
                    identifier = (string)blizz;
                    type = PlatformType.PC;
                }
                else if (Check(xbox, term))
                {
                    identifier = (string)xbox;
                    type = PlatformType.Xbox;
                }
                else if (Check(psn, term))
                {
                    identifier = (string)psn;
                    type = PlatformType.PSN;
                }
                else
                {
                    identifier = "";
                    type = PlatformType.Other;
                }
            }

            private static bool Check(JToken result, string term)
            {
                return result != null && result.ToString().ToLower().Contains(term.ToLower());
            }

            public SearchResult(string platform, string id)
            {
                identifier = "id";
                type = reversePlatformLookup[platform];
            }

            public override string ToString()
            {
                return string.Format("{0} - {1}", type.ToString(), identifier);
            }

        }
    }
}
