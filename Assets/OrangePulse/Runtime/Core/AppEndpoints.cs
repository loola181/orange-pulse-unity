namespace OrangePulse.Core
{
    public static class AppEndpoints
    {
        public const string SportsApiRoot = "https://www.thesportsdb.com/api/v1/json/123";
        public const string CampaignConfig =
            "https://raw.githubusercontent.com/loola181/orange-pulse-unity/refs/heads/main/Remote/banner.json";

        public static readonly LeagueSource[] FeaturedLeagues =
        {
            new("4328", "Премьер-лига", "ENG"),
            new("4335", "Ла Лига", "ESP"),
            new("4331", "Бундеслига", "GER")
        };

        public static string MatchesFor(string leagueId) =>
            $"{SportsApiRoot}/eventsnextleague.php?id={leagueId}";

        public static string NewsFor(NewsSection section) => section switch
        {
            NewsSection.Football => "https://feeds.bbci.co.uk/sport/football/rss.xml",
            NewsSection.FormulaOne => "https://feeds.bbci.co.uk/sport/formula1/rss.xml",
            NewsSection.Tennis => "https://feeds.bbci.co.uk/sport/tennis/rss.xml",
            _ => "https://feeds.bbci.co.uk/sport/rss.xml"
        };
    }

    public readonly struct LeagueSource
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Region;

        public LeagueSource(string id, string name, string region)
        {
            Id = id;
            Name = name;
            Region = region;
        }
    }

    public enum NewsSection
    {
        Football,
        FormulaOne,
        Tennis
    }
}
