using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using OrangePulse.Core;
using UnityEngine;

namespace OrangePulse.Data
{
    public sealed class MatchResultsGateway
    {
        private readonly HttpTransport _transport;
        private readonly DiskTextCache _cache;

        public MatchResultsGateway(HttpTransport transport, DiskTextCache cache)
        {
            _transport = transport;
            _cache = cache;
        }

        public IEnumerator LoadFeatured(Action<LoadResult<IReadOnlyList<MatchResult>>> finished)
        {
            var results = new List<MatchResult>();
            bool usedCache = false;
            string lastError = null;

            foreach (LeagueSource league in AppEndpoints.FeaturedLeagues)
            {
                LoadResult<string> response = null;
                yield return _transport.GetText(AppEndpoints.ResultsFor(league.Id), value => response = value);

                string payload = null;
                if (response != null && response.IsSuccess)
                {
                    payload = response.Data;
                    _cache.Put("results-" + league.Id, payload);
                }
                else if (_cache.TryGet("results-" + league.Id, TimeSpan.FromDays(30), out string cached))
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
                    results.AddRange(Parse(payload, league));
                }
                catch (Exception exception)
                {
                    lastError = exception.Message;
                }
            }

            if (results.Count == 0)
            {
                finished?.Invoke(LoadResult<IReadOnlyList<MatchResult>>.Failed(
                    string.IsNullOrWhiteSpace(lastError) ? "Результаты пока не опубликованы" : lastError));
                yield break;
            }

            results.Sort((left, right) => right.PlayedUtc.CompareTo(left.PlayedUtc));
            finished?.Invoke(usedCache
                ? LoadResult<IReadOnlyList<MatchResult>>.Cached(results)
                : LoadResult<IReadOnlyList<MatchResult>>.Fresh(results));
        }

        public static IReadOnlyList<MatchResult> Parse(string json, LeagueSource league)
        {
            SportsEnvelope envelope = JsonUtility.FromJson<SportsEnvelope>(json);
            var results = new List<MatchResult>();
            foreach (SportsEventDto source in envelope?.events ?? Array.Empty<SportsEventDto>())
            {
                if (source == null || !TryScore(source.intHomeScore, out int homeScore) ||
                    !TryScore(source.intAwayScore, out int awayScore)) continue;

                results.Add(new MatchResult
                {
                    Id = source.idEvent ?? string.Empty,
                    League = string.IsNullOrWhiteSpace(source.strLeague) ? league.Name : source.strLeague,
                    Region = league.Region,
                    HomeTeam = source.strHomeTeam ?? "Home",
                    AwayTeam = source.strAwayTeam ?? "Away",
                    HomeBadgeUrl = source.strHomeTeamBadge ?? string.Empty,
                    AwayBadgeUrl = source.strAwayTeamBadge ?? string.Empty,
                    HomeScore = homeScore,
                    AwayScore = awayScore,
                    Status = NormalizeStatus(source.strStatus),
                    PlayedUtc = ParseDate(source.strTimestamp, source.dateEvent)
                });
            }
            return results;
        }

        private static bool TryScore(string value, out int score) =>
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out score);

        private static DateTime ParseDate(string timestamp, string date)
        {
            if (DateTime.TryParse(timestamp, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime exact))
                return exact;
            return DateTime.TryParse(date, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime fallback)
                ? fallback
                : DateTime.UtcNow;
        }

        private static string NormalizeStatus(string value) => (value ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "AET" => "ДОП. ВРЕМЯ",
            "PEN" => "ПЕНАЛЬТИ",
            "FT" => "ЗАВЕРШЁН",
            _ => "ФИНАЛ"
        };
    }
}
