using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using OrangePulse.Core;

namespace OrangePulse.Data
{
    public sealed class NewsFeedGateway
    {
        private readonly HttpTransport _transport;
        private readonly DiskTextCache _cache;

        public NewsFeedGateway(HttpTransport transport, DiskTextCache cache)
        {
            _transport = transport;
            _cache = cache;
        }

        public IEnumerator Load(NewsSection section, Action<LoadResult<IReadOnlyList<NewsStory>>> finished)
        {
            LoadResult<string> response = null;
            yield return _transport.GetText(AppEndpoints.NewsFor(section), value => response = value);

            string cacheKey = "news-" + section.ToString().ToLowerInvariant();
            string payload;
            bool fromCache = false;

            if (response != null && response.IsSuccess)
            {
                payload = response.Data;
                _cache.Put(cacheKey, payload);
            }
            else if (_cache.TryGet(cacheKey, TimeSpan.FromDays(3), out string cached))
            {
                payload = cached;
                fromCache = true;
            }
            else
            {
                finished?.Invoke(LoadResult<IReadOnlyList<NewsStory>>.Failed(
                    response?.Error ?? "Лента новостей недоступна"));
                yield break;
            }

            try
            {
                IReadOnlyList<NewsStory> stories = Parse(payload);
                finished?.Invoke(fromCache
                    ? LoadResult<IReadOnlyList<NewsStory>>.Cached(stories)
                    : LoadResult<IReadOnlyList<NewsStory>>.Fresh(stories));
            }
            catch (Exception exception)
            {
                finished?.Invoke(LoadResult<IReadOnlyList<NewsStory>>.Failed(exception.Message));
            }
        }

        public static IReadOnlyList<NewsStory> Parse(string xml)
        {
            XDocument document = XDocument.Parse(xml, LoadOptions.None);
            var stories = new List<NewsStory>();

            foreach (XElement item in document.Descendants("item").Take(20))
            {
                string title = Value(item, "title");
                string url = Value(item, "link");
                if (string.IsNullOrWhiteSpace(title) || !IsSafeHttps(url)) continue;

                DateTime.TryParse(Value(item, "pubDate"), CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out DateTime published);

                stories.Add(new NewsStory
                {
                    Title = title.Trim(),
                    Summary = StripMarkup(Value(item, "description")),
                    Url = url.Trim(),
                    Source = "BBC SPORT",
                    PublishedUtc = published
                });
            }

            if (stories.Count == 0) throw new FormatException("В RSS нет доступных публикаций");
            return stories;
        }

        private static string Value(XElement item, string name) =>
            item.Element(name)?.Value ?? string.Empty;

        private static string StripMarkup(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Открыть материал и узнать подробности.";
            string plain = System.Text.RegularExpressions.Regex.Replace(value, "<.*?>", string.Empty);
            plain = System.Net.WebUtility.HtmlDecode(plain).Trim();
            return plain.Length > 180 ? plain.Substring(0, 177) + "..." : plain;
        }

        private static bool IsSafeHttps(string value) =>
            Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme == Uri.UriSchemeHttps;
    }
}

