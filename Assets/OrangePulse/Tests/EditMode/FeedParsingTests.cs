using System;
using NUnit.Framework;
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
        public void CampaignParserMapsServerPayload()
        {
            const string json = "{\"enabled\":true,\"eyebrow\":\"HOT\",\"title\":\"Final\"," +
                "\"body\":\"Details\",\"button_label\":\"Open\"," +
                "\"button_url\":\"https://example.com/event\"," +
                "\"image_url\":\"https://example.com/banner.jpg\"}";

            Campaign campaign = CampaignGateway.Parse(json);

            Assert.That(campaign.Enabled, Is.True);
            Assert.That(campaign.Title, Is.EqualTo("Final"));
            Assert.That(campaign.ButtonLabel, Is.EqualTo("Open"));
        }

        [Test]
        public void CampaignParserRejectsInsecureAction()
        {
            const string json = "{\"enabled\":true,\"button_url\":\"http://example.com\"}";
            Assert.Throws<FormatException>(() => CampaignGateway.Parse(json));
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

