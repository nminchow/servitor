namespace servitor.DestinyClient
{
    struct MembershipData
    {
        public Client.PlatformType MembershipType { get; set; }
        public string MembershipId { get; set; }

        public MembershipData(string type, string id)
        {
            MembershipId = id;
            MembershipType = Client.reversePlatformLookup[type];
        }
    }
}
