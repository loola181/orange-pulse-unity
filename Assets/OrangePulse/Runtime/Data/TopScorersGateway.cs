using System;
using System.Collections;
using System.Collections.Generic;
using OrangePulse.Core;
using UnityEngine;

namespace OrangePulse.Data
{
    public sealed class TopScorersGateway
    {
        private readonly HttpTransport _transport;
        private readonly DiskTextCache _cache;

        public TopScorersGateway(HttpTransport transport, DiskTextCache cache)
        {
            _transport = transport;
            _cache = cache;
        }

        public IEnumerator Load(LeagueSource league, Action<LoadResult<IReadOnlyList<ScorerRow>>> finished)
        {
            int season = StandingsGateway.CompletedSeasonStartYear(DateTime.UtcNow);
            string cacheKey = $"top-scorers-{league.FootballApiId}-{season}";
            LoadResult<string> response = null;
            yield return _transport.GetText(AppEndpoints.TopScorersFor(league.FootballApiId, season),
                value => response = value);

            string payload = null;
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

            IReadOnlyList<ScorerRow> rows = ParseSafe(payload);
            if (rows.Count == 0)
            {
                finished?.Invoke(LoadResult<IReadOnlyList<ScorerRow>>.Failed(
                    response?.Error ?? "Список бомбардиров пока недоступен"));
                yield break;
            }

            finished?.Invoke(fromCache
                ? LoadResult<IReadOnlyList<ScorerRow>>.Cached(rows)
                : LoadResult<IReadOnlyList<ScorerRow>>.Fresh(rows));
        }

        public static IReadOnlyList<ScorerRow> Parse(string json)
        {
            FootballTopScorersEnvelope envelope = JsonUtility.FromJson<FootballTopScorersEnvelope>(json);
            var rows = new List<ScorerRow>();
            foreach (FootballScorerDto source in envelope?.response ?? Array.Empty<FootballScorerDto>())
            {
                FootballPlayerStatisticsDto stats = source?.statistics != null && source.statistics.Length > 0
                    ? source.statistics[0]
                    : null;
                if (source?.player == null || stats?.team == null || stats.goals == null ||
                    string.IsNullOrWhiteSpace(source.player.name)) continue;

                rows.Add(new ScorerRow
                {
                    Player = source.player.name.Trim(),
                    PlayerPhotoUrl = source.player.photo ?? string.Empty,
                    Nationality = source.player.nationality ?? string.Empty,
                    Team = stats.team.name ?? string.Empty,
                    TeamBadgeUrl = stats.team.logo ?? string.Empty,
                    Appearances = stats.games?.appearences ?? 0,
                    Goals = stats.goals.total,
                    Assists = stats.goals.assists
                });
            }

            rows.Sort((left, right) =>
            {
                int goals = right.Goals.CompareTo(left.Goals);
                if (goals != 0) return goals;
                int assists = right.Assists.CompareTo(left.Assists);
                return assists != 0 ? assists : string.Compare(left.Player, right.Player, StringComparison.Ordinal);
            });
            for (int index = 0; index < rows.Count; index++) rows[index].Rank = index + 1;
            return rows;
        }

        private static IReadOnlyList<ScorerRow> ParseSafe(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return Array.Empty<ScorerRow>();
            try { return Parse(json); }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangeFootball] Top scorers parse failed: {exception.Message}");
                return Array.Empty<ScorerRow>();
            }
        }
    }
}
