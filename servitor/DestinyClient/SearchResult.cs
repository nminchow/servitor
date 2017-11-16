using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace servitor.DestinyClient
{
    public partial class Client
    {
        public static Dictionary<PlatformType, string> platformLookup = new Dictionary<PlatformType, string>
        {
            {PlatformType.PC, "4"},
            {PlatformType.Xbox, "1"},
            {PlatformType.PSN, "2"},
            {PlatformType.Other, "-1"}
        };

        public static Dictionary<string, PlatformType> reversePlatformLookup = platformLookup.ToDictionary(x => x.Value, x => x.Key);

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

            public SearchResult(JToken queryResult)
            {
                if (queryResult["blizzardDisplayName"] != null)
                {
                    identifier = (string)queryResult["blizzardDisplayName"];
                    type = PlatformType.PC;
                }
                else if (queryResult["xboxDisplayName"] != null)
                {
                    identifier = (string)queryResult["xboxDisplayName"];
                    type = PlatformType.Xbox;
                }
                else if (queryResult["psnDisplayName"] != null)
                {
                    identifier = (string)queryResult["psnDisplayName"];
                    type = PlatformType.PSN;
                }
                else
                {
                    identifier = "";
                    type = PlatformType.Other;
                }
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
