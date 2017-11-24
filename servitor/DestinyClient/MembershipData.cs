using static servitor.DestinyClient.SearchResultWrapper;

namespace servitor.DestinyClient
{
    public struct MembershipData
    {
        public PlatformType MembershipType { get; set; }
        public string MembershipId { get; set; }
        public string DisplayName { get; set; }

        public MembershipData(string type, string id, string displayName)
        {
            MembershipId = id;
            MembershipType = reversePlatformLookup[type];
            DisplayName = displayName;
        }
    }
}
