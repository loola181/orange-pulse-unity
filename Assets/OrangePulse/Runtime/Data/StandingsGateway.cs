using System;
using System.Collections;
using System.Collections.Generic;
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
            int season = CompletedSeasonStartYear(DateTime.UtcNow);
            LoadResult<string> response = null;
            yield return LoadPayload(AppEndpoints.StandingsFor(league.FootballApiId, season),
                $"football-standings-{league.FootballApiId}-{season}", value => response = value);

            IReadOnlyList<StandingRow> rows = ParseSafe(response?.Data);

            if (rows.Count == 0)
            {
                finished?.Invoke(LoadResult<IReadOnlyList<StandingRow>>.Failed(
                    response?.Error ?? "Таблица пока недоступна"));
                yield break;
            }

            finished?.Invoke(response.FromCache
                ? LoadResult<IReadOnlyList<StandingRow>>.Cached(rows)
                : LoadResult<IReadOnlyList<StandingRow>>.Fresh(rows));
        }

        public static IReadOnlyList<StandingRow> Parse(string json)
        {
            var rows = new List<StandingRow>();
            foreach (string item in ExtractStandingObjects(json))
            {
                FootballStandingDto source = JsonUtility.FromJson<FootballStandingDto>(item);
                if (source?.team == null || string.IsNullOrWhiteSpace(source.team.name)) continue;
                rows.Add(new StandingRow
                {
                    Rank = source.rank,
                    Team = source.team.name.Trim(),
                    BadgeUrl = source.team.logo ?? string.Empty,
                    Played = source.all?.played ?? 0,
                    Won = source.all?.win ?? 0,
                    Drawn = source.all?.draw ?? 0,
                    Lost = source.all?.lose ?? 0,
                    GoalDifference = source.goalsDiff,
                    Points = source.points
                });
            }
            rows.Sort((left, right) => left.Rank.CompareTo(right.Rank));
            return rows;
        }

        public static int CompletedSeasonStartYear(DateTime utcNow)
        {
            return utcNow.Month >= 7 ? utcNow.Year - 1 : utcNow.Year - 2;
        }

        private static IEnumerable<string> ExtractStandingObjects(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) yield break;
            int key = json.IndexOf("\"standings\":[", StringComparison.Ordinal);
            if (key < 0) yield break;
            int arrayStart = json.IndexOf('[', key);
            if (arrayStart < 0) yield break;

            int arrayDepth = 0;
            int objectDepth = 0;
            int objectStart = -1;
            bool quoted = false;
            bool escaped = false;

            for (int index = arrayStart; index < json.Length; index++)
            {
                char symbol = json[index];
                if (quoted)
                {
                    if (escaped) escaped = false;
                    else if (symbol == '\\') escaped = true;
                    else if (symbol == '"') quoted = false;
                    continue;
                }

                if (symbol == '"')
                {
                    quoted = true;
                    continue;
                }

                if (symbol == '[' && objectDepth == 0) arrayDepth++;
                else if (symbol == ']' && objectDepth == 0)
                {
                    arrayDepth--;
                    if (arrayDepth <= 0) yield break;
                }
                else if (symbol == '{')
                {
                    if (objectDepth == 0 && arrayDepth >= 2) objectStart = index;
                    objectDepth++;
                }
                else if (symbol == '}' && objectDepth > 0)
                {
                    objectDepth--;
                    if (objectDepth == 0 && objectStart >= 0)
                    {
                        yield return json.Substring(objectStart, index - objectStart + 1);
                        objectStart = -1;
                    }
                }
            }
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

    }
}
