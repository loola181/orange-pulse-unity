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
        public long id;
        public string name;
        public string logo;
    }

    [Serializable]
    public sealed class FootballTopScorersEnvelope
    {
        public FootballScorerDto[] response;
    }

    [Serializable]
    public sealed class FootballScorerDto
    {
        public FootballPlayerDto player;
        public FootballPlayerStatisticsDto[] statistics;
    }

    [Serializable]
    public sealed class FootballPlayerDto
    {
        public long id;
        public string name;
        public string nationality;
        public string photo;
    }

    [Serializable]
    public sealed class FootballPlayerStatisticsDto
    {
        public FootballTeamDto team;
        public FootballGamesDto games;
        public FootballGoalsDto goals;
    }

    [Serializable]
    public sealed class FootballGamesDto
    {
        public int appearences;
        public int minutes;
        public string position;
        public string rating;
    }

    [Serializable]
    public sealed class FootballGoalsDto
    {
        public int total;
        public int assists;
    }

    public sealed class ScorerRow
    {
        public int Rank;
        public string Player;
        public string PlayerPhotoUrl;
        public string Nationality;
        public string Team;
        public string TeamBadgeUrl;
        public int Appearances;
        public int Goals;
        public int Assists;
    }

    [Serializable]
    public sealed class FootballLineupsEnvelope
    {
        public FootballLineupDto[] response;
    }

    [Serializable]
    public sealed class FootballLineupDto
    {
        public FootballTeamDto team;
        public string formation;
        public FootballLineupSlotDto[] startXI;
    }

    [Serializable]
    public sealed class FootballLineupSlotDto
    {
        public FootballLineupPlayerDto player;
    }

    [Serializable]
    public sealed class FootballLineupPlayerDto
    {
        public long id;
        public string name;
        public int number;
        public string pos;
    }

    [Serializable]
    public sealed class FootballStatisticsEnvelope
    {
        public FootballTeamStatisticsDto[] response;
    }

    [Serializable]
    public sealed class FootballTeamStatisticsDto
    {
        public FootballTeamDto team;
        public FootballStatisticDto[] statistics;
    }

    [Serializable]
    public sealed class FootballStatisticDto
    {
        public string type;
        public string value;
    }

    [Serializable]
    public sealed class FootballEventsEnvelope
    {
        public FootballEventDto[] response;
    }

    [Serializable]
    public sealed class FootballEventDto
    {
        public FootballEventTimeDto time;
        public FootballTeamDto team;
        public FootballEventPlayerDto player;
        public FootballEventPlayerDto assist;
        public string type;
        public string detail;
        public string comments;
    }

    [Serializable]
    public sealed class FootballEventTimeDto
    {
        public int elapsed;
        public int extra;
    }

    [Serializable]
    public sealed class FootballEventPlayerDto
    {
        public long id;
        public string name;
    }

    public sealed class MatchCenterData
    {
        public MatchCenterLineup[] Lineups = Array.Empty<MatchCenterLineup>();
        public MatchMetric[] Metrics = Array.Empty<MatchMetric>();
        public MatchTimelineEvent[] Events = Array.Empty<MatchTimelineEvent>();
    }

    public sealed class MatchCenterLineup
    {
        public string Team;
        public string Formation;
        public string BadgeUrl;
        public LineupPlayer[] Starters = Array.Empty<LineupPlayer>();
    }

    public sealed class LineupPlayer
    {
        public string Name;
        public int Number;
        public string Position;
    }

    public sealed class MatchMetric
    {
        public string Label;
        public string HomeValue;
        public string AwayValue;
    }

    public sealed class MatchTimelineEvent
    {
        public int Minute;
        public int ExtraMinute;
        public string Team;
        public string Player;
        public string Assist;
        public string Type;
        public string Detail;
        public string Comments;
    }

    public sealed class MatchResult
    {
        public string Id;
        public string League;
        public string Region;
        public string HomeTeam;
        public string AwayTeam;
        public string HomeBadgeUrl;
        public string AwayBadgeUrl;
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
        public string favoriteLeagueId = "4328";
        public int openedStories;
        public int refreshedFeeds;
        public int openedMatchCenters;
        public int openedScorers;
    }
}
