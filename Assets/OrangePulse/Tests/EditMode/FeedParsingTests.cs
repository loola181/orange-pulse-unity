using System;
using NUnit.Framework;
using OrangePulse.Core;
using OrangePulse.Data;

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
                "\"intAwayScore\":\"1\",\"strStatus\":\"FT\"}]}";

            var results = MatchResultsGateway.Parse(json, new LeagueSource("4328", "АПЛ", "ENG"));

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].HomeScore, Is.EqualTo(3));
            Assert.That(results[0].AwayTeam, Is.EqualTo("Chelsea"));
            Assert.That(results[0].Status, Is.EqualTo("ЗАВЕРШЁН"));
        }

        [Test]
        public void StandingsParserOrdersRowsByRank()
        {
            const string json = "{\"table\":[" +
                "{\"intRank\":\"2\",\"strTeam\":\"City\",\"intPlayed\":\"38\",\"intPoints\":\"78\"}," +
                "{\"intRank\":\"1\",\"strTeam\":\"Arsenal\",\"intPlayed\":\"38\",\"intPoints\":\"85\"}]}";

            var rows = StandingsGateway.Parse(json);

            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows[0].Team, Is.EqualTo("Arsenal"));
            Assert.That(rows[0].Points, Is.EqualTo(85));
            Assert.That(StandingsGateway.PreviousCompletedSeason(new DateTime(2026, 7, 21)),
                Is.EqualTo("2025-2026"));
        }

        [Test]
        public void ProfileNameIsTrimmedAndLimited()
        {
            string value = ProfileStore.NormalizeName("   very-long-orange-profile-name   ");
            Assert.That(value, Has.Length.EqualTo(24));
            Assert.That(value, Does.Not.StartWith(" "));
        }
    }
}
