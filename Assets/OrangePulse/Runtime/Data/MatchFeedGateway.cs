using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using OrangePulse.Core;
using UnityEngine;

namespace OrangePulse.Data
{
    public sealed class MatchFeedGateway
    {
        private const int MatchesPerLeague = 4;
        private readonly HttpTransport _transport;
        private readonly DiskTextCache _cache;

        public MatchFeedGateway(HttpTransport transport, DiskTextCache cache)
        {
            _transport = transport;
            _cache = cache;
        }

        public IEnumerator LoadFeatured(Action<LoadResult<IReadOnlyList<MatchSummary>>> finished)
        {
            var matches = new List<MatchSummary>();
            bool usedCache = false;
            string lastError = null;

            foreach (LeagueSource league in AppEndpoints.FeaturedLeagues)
            {
                LoadResult<string> response = null;
                yield return _transport.GetText(
                    AppEndpoints.UpcomingMatchesFor(league.FootballApiId, MatchesPerLeague),
                    value => response = value);

                string payload = null;
                if (response != null && response.IsSuccess)
                {
                    payload = response.Data;
                    _cache.Put("football-matches-" + league.FootballApiId, payload);
                }
                else if (_cache.TryGet("football-matches-" + league.FootballApiId,
                             TimeSpan.FromDays(3), out string cached))
                {
                    payload = cached;
                    usedCache = true;
                }
                else
                {
                    lastError = response?.Error ?? "Нет ответа от сервера";
                }

                if (string.IsNullOrWhiteSpace(payload)) continue;

                try
                {
                    matches.AddRange(Parse(payload, league));
                }
                catch (Exception exception)
                {
                    lastError = exception.Message;
                }
            }

            if (matches.Count == 0)
            {
                finished?.Invoke(LoadResult<IReadOnlyList<MatchSummary>>.Failed(
                    string.IsNullOrWhiteSpace(lastError) ? "Матчи пока не опубликованы" : lastError));
                yield break;
            }

            matches.Sort((left, right) => left.KickoffUtc.CompareTo(right.KickoffUtc));
            finished?.Invoke(usedCache
                ? LoadResult<IReadOnlyList<MatchSummary>>.Cached(matches)
                : LoadResult<IReadOnlyList<MatchSummary>>.Fresh(matches));
        }

        public static IReadOnlyList<MatchSummary> Parse(string json, LeagueSource league)
        {
            FootballFixtureEnvelope envelope = JsonUtility.FromJson<FootballFixtureEnvelope>(json);
            var matches = new List<MatchSummary>();
            foreach (FootballFixtureDto source in envelope?.response ?? Array.Empty<FootballFixtureDto>())
            {
                if (source?.fixture == null || source.teams?.home == null || source.teams.away == null)
                    continue;

                matches.Add(new MatchSummary
                {
                    Id = source.fixture.id.ToString(CultureInfo.InvariantCulture),
                    League = league.Name,
                    Region = league.Region,
                    HomeTeam = source.teams.home.name ?? "Home",
                    AwayTeam = source.teams.away.name ?? "Away",
                    HomeBadgeUrl = source.teams.home.logo ?? string.Empty,
                    AwayBadgeUrl = source.teams.away.logo ?? string.Empty,
                    Venue = string.IsNullOrWhiteSpace(source.fixture.venue?.name)
                        ? "Стадион уточняется"
                        : source.fixture.venue.name,
                    KickoffUtc = ParseUtc(source.fixture.date)
                });
            }
            return matches;
        }

        private static DateTime ParseUtc(string timestamp)
        {
            if (DateTime.TryParse(timestamp, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime exact))
                return exact;
            return DateTime.UtcNow;
        }
    }
}
