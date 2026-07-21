using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OrangePulse.Core;
using UnityEngine;

namespace OrangePulse.Data
{
    public sealed class StandingsGateway
    {
        private readonly HttpTransport _transport;
        private readonly DiskTextCache _cache;

        public StandingsGateway(HttpTransport transport, DiskTextCache cache)
        {
            _transport = transport;
            _cache = cache;
        }

        public IEnumerator Load(LeagueSource league,
            Action<LoadResult<IReadOnlyList<StandingRow>>> finished)
        {
            LoadResult<string> current = null;
            yield return LoadPayload(AppEndpoints.StandingsFor(league.Id), "standings-current-" + league.Id,
                value => current = value);

            IReadOnlyList<StandingRow> rows = ParseSafe(current?.Data);
            bool fromCache = current?.FromCache == true;
            if (rows.Count == 0 || rows.All(row => row.Played == 0))
            {
                string season = PreviousCompletedSeason(DateTime.UtcNow);
                LoadResult<string> previous = null;
                yield return LoadPayload(AppEndpoints.StandingsFor(league.Id, season),
                    $"standings-{league.Id}-{season}", value => previous = value);
                IReadOnlyList<StandingRow> completed = ParseSafe(previous?.Data);
                if (completed.Count > 0)
                {
                    rows = completed;
                    fromCache = previous.FromCache;
                }
            }

            if (rows.Count == 0)
            {
                finished?.Invoke(LoadResult<IReadOnlyList<StandingRow>>.Failed(
                    current?.Error ?? "Таблица пока недоступна"));
                yield break;
            }

            finished?.Invoke(fromCache
                ? LoadResult<IReadOnlyList<StandingRow>>.Cached(rows)
                : LoadResult<IReadOnlyList<StandingRow>>.Fresh(rows));
        }

        public static IReadOnlyList<StandingRow> Parse(string json)
        {
            LeagueTableEnvelope envelope = JsonUtility.FromJson<LeagueTableEnvelope>(json);
            var rows = new List<StandingRow>();
            foreach (LeagueTableDto source in envelope?.table ?? Array.Empty<LeagueTableDto>())
            {
                if (source == null || string.IsNullOrWhiteSpace(source.strTeam)) continue;
                rows.Add(new StandingRow
                {
                    Rank = Number(source.intRank),
                    Team = source.strTeam.Trim(),
                    BadgeUrl = source.strBadge ?? string.Empty,
                    Played = Number(source.intPlayed),
                    Won = Number(source.intWin),
                    Drawn = Number(source.intDraw),
                    Lost = Number(source.intLoss),
                    GoalDifference = Number(source.intGoalDifference),
                    Points = Number(source.intPoints)
                });
            }
            rows.Sort((left, right) => left.Rank.CompareTo(right.Rank));
            return rows;
        }

        public static string PreviousCompletedSeason(DateTime utcNow)
        {
            int startYear = utcNow.Month >= 7 ? utcNow.Year - 1 : utcNow.Year - 2;
            return $"{startYear}-{startYear + 1}";
        }

        private IEnumerator LoadPayload(string url, string cacheKey, Action<LoadResult<string>> finished)
        {
            LoadResult<string> response = null;
            yield return _transport.GetText(url, value => response = value);
            if (response != null && response.IsSuccess)
            {
                _cache.Put(cacheKey, response.Data);
                finished?.Invoke(response);
                yield break;
            }
            if (_cache.TryGet(cacheKey, TimeSpan.FromDays(14), out string cached))
            {
                finished?.Invoke(LoadResult<string>.Cached(cached));
                yield break;
            }
            finished?.Invoke(LoadResult<string>.Failed(response?.Error ?? "Нет ответа от сервера"));
        }

        private static IReadOnlyList<StandingRow> ParseSafe(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return Array.Empty<StandingRow>();
            try { return Parse(json); }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangeFootball] Standings parse failed: {exception.Message}");
                return Array.Empty<StandingRow>();
            }
        }

        private static int Number(string value) =>
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number) ? number : 0;
    }
}
