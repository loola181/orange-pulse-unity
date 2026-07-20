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
                yield return _transport.GetText(AppEndpoints.MatchesFor(league.Id), value => response = value);

                string payload = null;
                if (response != null && response.IsSuccess)
                {
                    payload = response.Data;
                    _cache.Put("matches-" + league.Id, payload);
                }
                else if (_cache.TryGet("matches-" + league.Id, TimeSpan.FromDays(7), out string cached))
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
                    SportsEnvelope envelope = JsonUtility.FromJson<SportsEnvelope>(payload);
                    SportsEventDto source = envelope?.events != null && envelope.events.Length > 0
                        ? envelope.events[0]
                        : null;
                    if (source != null) matches.Add(ToSummary(source, league));
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

        private static MatchSummary ToSummary(SportsEventDto source, LeagueSource league)
        {
            DateTime kickoff = ParseUtc(source.strTimestamp, source.dateEvent, source.strTime);
            return new MatchSummary
            {
                Id = source.idEvent,
                League = string.IsNullOrWhiteSpace(source.strLeague) ? league.Name : source.strLeague,
                Region = league.Region,
                HomeTeam = source.strHomeTeam ?? "Home",
                AwayTeam = source.strAwayTeam ?? "Away",
                HomeBadgeUrl = source.strHomeTeamBadge,
                AwayBadgeUrl = source.strAwayTeamBadge,
                Venue = string.IsNullOrWhiteSpace(source.strVenue) ? "Venue TBA" : source.strVenue,
                KickoffUtc = kickoff
            };
        }

        private static DateTime ParseUtc(string timestamp, string date, string time)
        {
            if (DateTime.TryParse(timestamp, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime exact))
                return exact;

            string combined = $"{date} {time}".Trim();
            return DateTime.TryParse(combined, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime fallback)
                ? fallback
                : DateTime.UtcNow;
        }
    }
}

