using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OrangePulse.Core;
using UnityEngine;

namespace OrangePulse.Data
{
    public sealed class MatchCenterGateway
    {
        private static readonly (string ApiName, string Label)[] MetricOrder =
        {
            ("Ball Possession", "ВЛАДЕНИЕ"),
            ("Shots on Goal", "УДАРЫ В СТВОР"),
            ("Total Shots", "ВСЕГО УДАРОВ"),
            ("Corner Kicks", "УГЛОВЫЕ"),
            ("Fouls", "ФОЛЫ"),
            ("Yellow Cards", "ЖЁЛТЫЕ КАРТОЧКИ"),
            ("Goalkeeper Saves", "СЕЙВЫ")
        };

        private readonly HttpTransport _transport;
        private readonly DiskTextCache _cache;

        public MatchCenterGateway(HttpTransport transport, DiskTextCache cache)
        {
            _transport = transport;
            _cache = cache;
        }

        public IEnumerator Load(string fixtureId, Action<LoadResult<MatchCenterData>> finished)
        {
            if (!long.TryParse(fixtureId, out long numericId) || numericId <= 0)
            {
                finished?.Invoke(LoadResult<MatchCenterData>.Failed("Некорректный идентификатор матча"));
                yield break;
            }

            LoadResult<string> lineups = null;
            LoadResult<string> statistics = null;
            LoadResult<string> events = null;
            yield return LoadPayload(AppEndpoints.FixtureLineups(fixtureId), $"center-lineups-{fixtureId}",
                value => lineups = value);
            yield return LoadPayload(AppEndpoints.FixtureStatistics(fixtureId), $"center-statistics-{fixtureId}",
                value => statistics = value);
            yield return LoadPayload(AppEndpoints.FixtureEvents(fixtureId), $"center-events-{fixtureId}",
                value => events = value);

            bool hasPayload = lineups?.IsSuccess == true || statistics?.IsSuccess == true || events?.IsSuccess == true;
            if (!hasPayload)
            {
                finished?.Invoke(LoadResult<MatchCenterData>.Failed(
                    events?.Error ?? statistics?.Error ?? lineups?.Error ?? "Детали матча недоступны"));
                yield break;
            }

            var data = new MatchCenterData
            {
                Lineups = ParseLineupsSafe(lineups?.Data),
                Metrics = ParseStatisticsSafe(statistics?.Data),
                Events = ParseEventsSafe(events?.Data)
            };
            bool fromCache = lineups?.FromCache == true || statistics?.FromCache == true || events?.FromCache == true;
            finished?.Invoke(fromCache
                ? LoadResult<MatchCenterData>.Cached(data)
                : LoadResult<MatchCenterData>.Fresh(data));
        }

        public static MatchCenterLineup[] ParseLineups(string json)
        {
            FootballLineupsEnvelope envelope = JsonUtility.FromJson<FootballLineupsEnvelope>(json);
            return (envelope?.response ?? Array.Empty<FootballLineupDto>())
                .Where(source => source?.team != null)
                .Select(source => new MatchCenterLineup
                {
                    Team = source.team.name ?? string.Empty,
                    BadgeUrl = source.team.logo ?? string.Empty,
                    Formation = source.formation ?? string.Empty,
                    Starters = (source.startXI ?? Array.Empty<FootballLineupSlotDto>())
                        .Where(slot => slot?.player != null)
                        .Select(slot => new LineupPlayer
                        {
                            Name = slot.player.name ?? string.Empty,
                            Number = slot.player.number,
                            Position = slot.player.pos ?? string.Empty
                        }).ToArray()
                }).ToArray();
        }

        public static MatchMetric[] ParseStatistics(string json)
        {
            string normalized = Regex.Replace(json ?? string.Empty,
                "(\\\"value\\\"\\s*:\\s*)(-?\\d+(?:\\.\\d+)?)", "$1\"$2\"");
            FootballStatisticsEnvelope envelope = JsonUtility.FromJson<FootballStatisticsEnvelope>(normalized);
            FootballTeamStatisticsDto[] teams = envelope?.response ?? Array.Empty<FootballTeamStatisticsDto>();
            if (teams.Length == 0) return Array.Empty<MatchMetric>();

            Dictionary<string, string> home = ToMetricMap(teams[0]?.statistics);
            Dictionary<string, string> away = teams.Length > 1
                ? ToMetricMap(teams[1]?.statistics)
                : new Dictionary<string, string>(StringComparer.Ordinal);

            var metrics = new List<MatchMetric>();
            foreach ((string apiName, string label) in MetricOrder)
            {
                home.TryGetValue(apiName, out string homeValue);
                away.TryGetValue(apiName, out string awayValue);
                if (string.IsNullOrWhiteSpace(homeValue) && string.IsNullOrWhiteSpace(awayValue)) continue;
                metrics.Add(new MatchMetric
                {
                    Label = label,
                    HomeValue = string.IsNullOrWhiteSpace(homeValue) ? "0" : homeValue,
                    AwayValue = string.IsNullOrWhiteSpace(awayValue) ? "0" : awayValue
                });
            }
            return metrics.ToArray();
        }

        public static MatchTimelineEvent[] ParseEvents(string json)
        {
            FootballEventsEnvelope envelope = JsonUtility.FromJson<FootballEventsEnvelope>(json);
            return (envelope?.response ?? Array.Empty<FootballEventDto>())
                .Where(source => source?.time != null)
                .Select(source => new MatchTimelineEvent
                {
                    Minute = source.time.elapsed,
                    ExtraMinute = source.time.extra,
                    Team = source.team?.name ?? string.Empty,
                    Player = source.player?.name ?? string.Empty,
                    Assist = source.assist?.name ?? string.Empty,
                    Type = source.type ?? string.Empty,
                    Detail = source.detail ?? string.Empty,
                    Comments = source.comments ?? string.Empty
                })
                .OrderBy(item => item.Minute)
                .ThenBy(item => item.ExtraMinute)
                .ToArray();
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
            if (_cache.TryGet(cacheKey, TimeSpan.FromHours(12), out string cached))
            {
                finished?.Invoke(LoadResult<string>.Cached(cached));
                yield break;
            }
            finished?.Invoke(LoadResult<string>.Failed(response?.Error ?? "Нет ответа от сервера"));
        }

        private static Dictionary<string, string> ToMetricMap(FootballStatisticDto[] source)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (FootballStatisticDto metric in source ?? Array.Empty<FootballStatisticDto>())
            {
                if (metric == null || string.IsNullOrWhiteSpace(metric.type)) continue;
                values[metric.type] = metric.value ?? string.Empty;
            }
            return values;
        }

        private static MatchCenterLineup[] ParseLineupsSafe(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return Array.Empty<MatchCenterLineup>();
            try { return ParseLineups(json); }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangeFootball] Lineups parse failed: {exception.Message}");
                return Array.Empty<MatchCenterLineup>();
            }
        }

        private static MatchMetric[] ParseStatisticsSafe(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return Array.Empty<MatchMetric>();
            try { return ParseStatistics(json); }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangeFootball] Statistics parse failed: {exception.Message}");
                return Array.Empty<MatchMetric>();
            }
        }

        private static MatchTimelineEvent[] ParseEventsSafe(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return Array.Empty<MatchTimelineEvent>();
            try { return ParseEvents(json); }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangeFootball] Events parse failed: {exception.Message}");
                return Array.Empty<MatchTimelineEvent>();
            }
        }
    }
}
