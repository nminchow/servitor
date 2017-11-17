using servitor.DestinyClient;

namespace servitor.Views
{
    static class ViewHelpers
    {
        public static string AsUrl(string path)
        {
            return Client.BungieRootPath + path;
        }
    }
}
