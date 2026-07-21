namespace OrangePulse.Core
{
    public static class AppEndpoints
    {
        public const string SportsApiRoot = "https://www.thesportsdb.com/api/v1/json/123";
        public const string FootballDataRoot =
            "https://europe-west1-fest-58d28.cloudfunctions.net/dataApi/football";
        public const string PrivacyPolicyUrl =
            "https://loola181.github.io/orange-pulse-unity/privacy-policy.html";

        public static readonly LeagueSource[] FeaturedLeagues =
        {
            new("4328", "39", "Премьер-лига", "ENG"),
            new("4335", "140", "Ла Лига", "ESP"),
            new("4331", "78", "Бундеслига", "GER")
        };

        public static string UpcomingMatchesFor(string footballApiLeagueId, int count) =>
            $"{FootballDataRoot}/fixtures?league={footballApiLeagueId}&next={count}";

        public static string ResultsFor(string leagueId) =>
            $"{SportsApiRoot}/eventspastleague.php?id={leagueId}";

        public static string StandingsFor(string footballApiLeagueId, int season) =>
            $"{FootballDataRoot}/standings?league={footballApiLeagueId}&season={season}";

        public static string TopScorersFor(string footballApiLeagueId, int season) =>
            $"{FootballDataRoot}/players/topscorers?league={footballApiLeagueId}&season={season}";

        public static string FixtureLineups(string fixtureId) =>
            $"{FootballDataRoot}/fixtures/lineups?fixture={fixtureId}";

        public static string FixtureStatistics(string fixtureId) =>
            $"{FootballDataRoot}/fixtures/statistics?fixture={fixtureId}";

        public static string FixtureEvents(string fixtureId) =>
            $"{FootballDataRoot}/fixtures/events?fixture={fixtureId}";

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
        public readonly string FootballApiId;
        public readonly string Name;
        public readonly string Region;

        public LeagueSource(string id, string footballApiId, string name, string region)
        {
            Id = id;
            FootballApiId = footballApiId;
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
