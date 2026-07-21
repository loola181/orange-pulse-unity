using System;

namespace OrangePulse.Data
{
    [Serializable]
    public sealed class SportsEnvelope
    {
        public SportsEventDto[] events;
    }

    [Serializable]
    public sealed class SportsEventDto
    {
        public string idEvent;
        public string strLeague;
        public string strTimestamp;
        public string dateEvent;
        public string strTime;
        public string strHomeTeam;
        public string strAwayTeam;
        public string strHomeTeamBadge;
        public string strAwayTeamBadge;
        public string strVenue;
        public string strStatus;
        public string intHomeScore;
        public string intAwayScore;
    }

    public sealed class MatchSummary
    {
        public string Id;
        public string League;
        public string Region;
        public string HomeTeam;
        public string AwayTeam;
        public string HomeBadgeUrl;
        public string AwayBadgeUrl;
        public string Venue;
        public DateTime KickoffUtc;
    }

    [Serializable]
    public sealed class FootballFixtureEnvelope
    {
        public FootballFixtureDto[] response;
    }

    [Serializable]
    public sealed class FootballFixtureDto
    {
        public FootballFixtureInfo fixture;
        public FootballLeagueInfo league;
        public FootballTeamsDto teams;
    }

    [Serializable]
    public sealed class FootballFixtureInfo
    {
        public long id;
        public string date;
        public FootballVenueDto venue;
    }

    [Serializable]
    public sealed class FootballVenueDto
    {
        public string name;
    }

    [Serializable]
    public sealed class FootballLeagueInfo
    {
        public string name;
    }

    [Serializable]
    public sealed class FootballTeamsDto
    {
        public FootballTeamDto home;
        public FootballTeamDto away;
    }

    [Serializable]
    public sealed class FootballTeamDto
    {
        public string name;
        public string logo;
    }

    public sealed class MatchResult
    {
        public string Id;
        public string League;
        public string Region;
        public string HomeTeam;
        public string AwayTeam;
        public int HomeScore;
        public int AwayScore;
        public string Status;
        public DateTime PlayedUtc;
    }

    [Serializable]
    public sealed class LeagueTableEnvelope
    {
        public LeagueTableDto[] table;
    }

    [Serializable]
    public sealed class LeagueTableDto
    {
        public string intRank;
        public string strTeam;
        public string strBadge;
        public string intPlayed;
        public string intWin;
        public string intDraw;
        public string intLoss;
        public string intGoalDifference;
        public string intPoints;
    }

    public sealed class StandingRow
    {
        public int Rank;
        public string Team;
        public string BadgeUrl;
        public int Played;
        public int Won;
        public int Drawn;
        public int Lost;
        public int GoalDifference;
        public int Points;
    }

    [Serializable]
    public sealed class FootballStandingDto
    {
        public int rank;
        public FootballTeamDto team;
        public int points;
        public int goalsDiff;
        public FootballStandingStatsDto all;
    }

    [Serializable]
    public sealed class FootballStandingStatsDto
    {
        public int played;
        public int win;
        public int draw;
        public int lose;
    }

    public sealed class NewsStory
    {
        public string Title;
        public string Summary;
        public string Url;
        public string Source;
        public DateTime PublishedUtc;
    }

    public sealed class Campaign
    {
        public bool Enabled;
        public string Eyebrow;
        public string Title;
        public string Body;
        public string ButtonLabel;
        public string ButtonUrl;
        public string ImageUrl;
        public string ImageRevision;
        public string DisplayMode;
    }

    [Serializable]
    public sealed class ProfileData
    {
        public string nickname = "Игрок 12";
        public string avatarPath = string.Empty;
        public int openedStories;
        public int refreshedFeeds;
    }
}
