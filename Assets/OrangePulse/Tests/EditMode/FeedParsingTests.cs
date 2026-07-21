using System;
using NUnit.Framework;
using OrangePulse.Core;
using OrangePulse.Data;
using OrangePulse.Presentation;

namespace OrangePulse.Tests
{
    public sealed class FeedParsingTests
    {
        [Test]
        public void NewsParserReadsSafeRssItems()
        {
            const string xml = "<?xml version=\"1.0\"?><rss><channel>" +
                "<item><title>Orange final</title><link>https://example.com/story</link>" +
                "<description><![CDATA[<p>Match report</p>]]></description>" +
                "<pubDate>Mon, 20 Jul 2026 18:00:00 GMT</pubDate></item>" +
                "</channel></rss>";

            var stories = NewsFeedGateway.Parse(xml);

            Assert.That(stories, Has.Count.EqualTo(1));
            Assert.That(stories[0].Title, Is.EqualTo("Orange final"));
            Assert.That(stories[0].Summary, Is.EqualTo("Match report"));
            Assert.That(stories[0].PublishedUtc.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void NewsParserDropsNonHttpsLinks()
        {
            const string xml = "<rss><channel><item><title>Unsafe</title>" +
                "<link>http://example.com/story</link></item></channel></rss>";

            Assert.Throws<FormatException>(() => NewsFeedGateway.Parse(xml));
        }

        [Test]
        public void CampaignMapperUsesNamespacedRemoteValues()
        {
            Campaign campaign = CampaignGateway.MapValues(
                true,
                true,
                "url",
                "https://example.com/event",
                "https://example.com/banner.jpg",
                "Final",
                "Details",
                "revision-1",
                "full_banner");

            Assert.That(campaign.Enabled, Is.True);
            Assert.That(campaign.Title, Is.EqualTo("Final"));
            Assert.That(campaign.ButtonLabel, Is.EqualTo("ОТКРЫТЬ"));
            Assert.That(campaign.ImageUrl, Is.EqualTo("https://example.com/banner.jpg"));
            Assert.That(campaign.DisplayMode, Is.EqualTo("full_banner"));
            Assert.That(campaign.Eyebrow, Does.Not.Contain("FIREBASE"));
        }

        [Test]
        public void CampaignMapperReplacesInsecureUrls()
        {
            Campaign campaign = CampaignGateway.MapValues(
                true,
                true,
                "url",
                "http://example.com",
                "http://example.com/banner.jpg",
                "Final",
                "Details");

            Assert.That(campaign.ButtonUrl, Does.StartWith("https://"));
            Assert.That(campaign.ImageUrl, Is.Empty);
        }

        [Test]
        public void MatchResultsParserReadsFinalScore()
        {
            const string json = "{\"events\":[{\"idEvent\":\"1\",\"strLeague\":\"Premier League\"," +
                "\"dateEvent\":\"2026-05-24\",\"strHomeTeam\":\"Arsenal\"," +
                "\"strAwayTeam\":\"Chelsea\",\"intHomeScore\":\"3\"," +
                "\"intAwayScore\":\"1\",\"strStatus\":\"FT\"," +
                "\"strHomeTeamBadge\":\"https://cdn.example.com/arsenal.png\"," +
                "\"strAwayTeamBadge\":\"https://cdn.example.com/chelsea.png\"}]}";

            var results = MatchResultsGateway.Parse(json, new LeagueSource("4328", "39", "АПЛ", "ENG"));

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].HomeScore, Is.EqualTo(3));
            Assert.That(results[0].AwayTeam, Is.EqualTo("Chelsea"));
            Assert.That(results[0].Status, Is.EqualTo("ЗАВЕРШЁН"));
            Assert.That(results[0].HomeBadgeUrl, Is.EqualTo("https://cdn.example.com/arsenal.png"));
            Assert.That(results[0].AwayBadgeUrl, Is.EqualTo("https://cdn.example.com/chelsea.png"));
        }

        [Test]
        public void StandingsParserOrdersRowsByRank()
        {
            const string json = "{\"get\":\"standings\",\"response\":[{\"league\":{\"standings\":[[" +
                "{\"rank\":2,\"team\":{\"name\":\"City\",\"logo\":\"city.png\"}," +
                "\"points\":78,\"goalsDiff\":42,\"all\":{\"played\":38,\"win\":23,\"draw\":9,\"lose\":6}}," +
                "{\"rank\":1,\"team\":{\"name\":\"Arsenal\",\"logo\":\"arsenal.png\"}," +
                "\"points\":85,\"goalsDiff\":44,\"all\":{\"played\":38,\"win\":26,\"draw\":7,\"lose\":5}}" +
                "]]}}]}";

            var rows = StandingsGateway.Parse(json);

            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows[0].Team, Is.EqualTo("Arsenal"));
            Assert.That(rows[0].Points, Is.EqualTo(85));
            Assert.That(rows[0].Played, Is.EqualTo(38));
            Assert.That(rows[0].BadgeUrl, Is.EqualTo("arsenal.png"));
            Assert.That(StandingsGateway.CompletedSeasonStartYear(new DateTime(2026, 7, 21)),
                Is.EqualTo(2025));
        }

        [Test]
        public void MatchFeedParserReadsAllUpcomingFixtures()
        {
            const string json = "{\"response\":[" +
                "{\"fixture\":{\"id\":101,\"date\":\"2026-08-21T19:00:00+00:00\"," +
                "\"venue\":{\"name\":\"Emirates Stadium\"}},\"league\":{\"name\":\"Premier League\"}," +
                "\"teams\":{\"home\":{\"name\":\"Arsenal\",\"logo\":\"a.png\"}," +
                "\"away\":{\"name\":\"Coventry\",\"logo\":\"c.png\"}}}," +
                "{\"fixture\":{\"id\":102,\"date\":\"2026-08-22T11:30:00+00:00\"," +
                "\"venue\":{\"name\":\"Craven Cottage\"}},\"league\":{\"name\":\"Premier League\"}," +
                "\"teams\":{\"home\":{\"name\":\"Fulham\"},\"away\":{\"name\":\"Leeds\"}}}]}";

            var matches = MatchFeedGateway.Parse(json,
                new LeagueSource("4328", "39", "Премьер-лига", "ENG"));

            Assert.That(matches, Has.Count.EqualTo(2));
            Assert.That(matches[0].HomeTeam, Is.EqualTo("Arsenal"));
            Assert.That(matches[1].Venue, Is.EqualTo("Craven Cottage"));
            Assert.That(matches[0].KickoffUtc.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(matches[0].HomeBadgeUrl, Is.EqualTo("a.png"));
            Assert.That(matches[0].AwayBadgeUrl, Is.EqualTo("c.png"));
        }

        [Test]
        public void ClubBadgeLoaderAcceptsOnlyHttpsUrls()
        {
            Assert.That(ClubBadgeLoader.IsSupportedUrl("https://cdn.example.com/club.png"), Is.True);
            Assert.That(ClubBadgeLoader.IsSupportedUrl("http://cdn.example.com/club.png"), Is.False);
            Assert.That(ClubBadgeLoader.IsSupportedUrl("club.png"), Is.False);
        }

        [Test]
        public void TopScorersParserOrdersGoalsAndAssists()
        {
            const string json = "{\"response\":[" +
                "{\"player\":{\"name\":\"Player B\",\"photo\":\"https://cdn/b.png\"}," +
                "\"statistics\":[{\"team\":{\"name\":\"Club B\",\"logo\":\"https://cdn/cb.png\"}," +
                "\"games\":{\"appearences\":30},\"goals\":{\"total\":18,\"assists\":9}}]}," +
                "{\"player\":{\"name\":\"Player A\",\"photo\":\"https://cdn/a.png\"}," +
                "\"statistics\":[{\"team\":{\"name\":\"Club A\",\"logo\":\"https://cdn/ca.png\"}," +
                "\"games\":{\"appearences\":28},\"goals\":{\"total\":21,\"assists\":4}}]}]}";

            var rows = TopScorersGateway.Parse(json);

            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows[0].Player, Is.EqualTo("Player A"));
            Assert.That(rows[0].Rank, Is.EqualTo(1));
            Assert.That(rows[0].Goals, Is.EqualTo(21));
            Assert.That(rows[1].TeamBadgeUrl, Is.EqualTo("https://cdn/cb.png"));
        }

        [Test]
        public void MatchCenterParsersReadLineupsMetricsAndTimeline()
        {
            const string lineups = "{\"response\":[{\"team\":{\"name\":\"Orange\",\"logo\":\"https://cdn/o.png\"}," +
                "\"formation\":\"4-3-3\",\"startXI\":[{\"player\":{\"name\":\"Keeper\",\"number\":1,\"pos\":\"G\"}}]}]}";
            const string statistics = "{\"response\":[" +
                "{\"statistics\":[{\"type\":\"Ball Possession\",\"value\":57},{\"type\":\"Total Shots\",\"value\":12}]}," +
                "{\"statistics\":[{\"type\":\"Ball Possession\",\"value\":\"43%\"},{\"type\":\"Total Shots\",\"value\":8}]}]}";
            const string events = "{\"response\":[" +
                "{\"time\":{\"elapsed\":70},\"team\":{\"name\":\"Black\"},\"player\":{\"name\":\"Late\"},\"type\":\"Card\",\"detail\":\"Yellow Card\"}," +
                "{\"time\":{\"elapsed\":12},\"team\":{\"name\":\"Orange\"},\"player\":{\"name\":\"Early\"},\"type\":\"Goal\",\"detail\":\"Normal Goal\"}]}";

            MatchCenterLineup[] parsedLineups = MatchCenterGateway.ParseLineups(lineups);
            MatchMetric[] parsedMetrics = MatchCenterGateway.ParseStatistics(statistics);
            MatchTimelineEvent[] parsedEvents = MatchCenterGateway.ParseEvents(events);

            Assert.That(parsedLineups[0].Formation, Is.EqualTo("4-3-3"));
            Assert.That(parsedLineups[0].Starters[0].Number, Is.EqualTo(1));
            Assert.That(parsedMetrics[0].HomeValue, Is.EqualTo("57"));
            Assert.That(parsedMetrics[0].AwayValue, Is.EqualTo("43%"));
            Assert.That(parsedEvents[0].Player, Is.EqualTo("Early"));
            Assert.That(parsedEvents[0].Minute, Is.EqualTo(12));
        }

        [Test]
        public void ProfileNameIsTrimmedAndLimited()
        {
            string value = ProfileStore.NormalizeName("   very-long-orange-profile-name   ");
            Assert.That(value, Has.Length.EqualTo(24));
            Assert.That(value, Does.Not.StartWith(" "));
        }

        [Test]
        public void ProfileProgressUsesAllSportsActivity()
        {
            var profile = new ProfileData
            {
                openedStories = 3,
                refreshedFeeds = 2,
                openedMatchCenters = 4,
                openedScorers = 4
            };

            Assert.That(ProfileStore.ActivityPoints(profile), Is.EqualTo(200));
            Assert.That(ProfileStore.Level(profile), Is.EqualTo(3));
            Assert.That(ProfileStore.LevelProgress(profile), Is.EqualTo(0));
            Assert.That(ProfileStore.LevelTitle(profile), Is.EqualTo("ЗНАТОК"));
            Assert.That(ProfileStore.NormalizeLeagueId("unknown"), Is.EqualTo("4328"));
        }
    }
}
