using System;
using System.Globalization;
using OrangePulse.Core;
using OrangePulse.Data;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation.Pages
{
    public sealed class MatchCenterPage : PageSurface
    {
        private readonly VisualComposer _ui;
        private readonly ClubBadgeLoader _images;
        private readonly RectTransform _content;
        private MatchSummary _match;

        public MatchCenterPage(VisualComposer ui, Transform parent, ClubBadgeLoader images, Action back)
        {
            _ui = ui;
            _images = images;
            Root = ui.Panel(parent, "MatchCenterPage", PulsePalette.Paper).rectTransform;
            VisualComposer.Stretch(Root);
            BuildHeader(back);
            ScrollRect scroll = ui.VerticalScroll(Root, "MatchCenterScroll", out _content);
            VisualComposer.SetAnchors(scroll.GetComponent<RectTransform>(), Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(0f, -146f));
        }

        public void ShowLoading(MatchSummary match)
        {
            _match = match;
            Clear();
            BuildMatchHero(match);
            Text loading = _ui.Label(_content, "Loading", "ЗАГРУЖАЕМ СОСТАВЫ И СТАТИСТИКУ...", 25,
                PulsePalette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(loading.gameObject, 150f);
        }

        public void Render(MatchSummary match, LoadResult<MatchCenterData> result)
        {
            if (_match == null || match == null || _match.Id != match.Id) return;
            Clear();
            BuildMatchHero(match);

            if (result == null || !result.IsSuccess || result.Data == null)
            {
                BuildNotice("ДЕТАЛИ ПОКА НЕДОСТУПНЫ", result?.Error ?? "Нет ответа от сервера", true);
                return;
            }

            Text status = _ui.Label(_content, "Status",
                result.FromCache ? "СОХРАНЁННЫЕ ДАННЫЕ МАТЧА" : "ДАННЫЕ МАТЧА · API LIVE", 21,
                result.FromCache ? PulsePalette.Muted : PulsePalette.Success,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.Size(status.gameObject, 58f);

            BuildTimeline(result.Data.Events);
            BuildMetrics(result.Data.Metrics);
            BuildLineups(result.Data.Lineups);

            Text credit = _ui.Label(_content, "Credit", "МАТЧ-ЦЕНТР: API-FOOTBALL", 19,
                PulsePalette.Muted, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(credit.gameObject, 86f);
        }

        private void BuildHeader(Action back)
        {
            Image header = _ui.Panel(Root, "Header", PulsePalette.Ink);
            VisualComposer.SetAnchors(header.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -146f), Vector2.zero);
            Button backButton = _ui.Button(header.transform, "Back", "‹ НАЗАД", Color.clear,
                PulsePalette.Orange, 23, back, false);
            VisualComposer.SetAnchors(backButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(26f, -34f), new Vector2(190f, 34f));
            Text title = _ui.Label(header.transform, "Title", "МАТЧ-ЦЕНТР", 43,
                PulsePalette.White, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(210f, 30f), new Vector2(-40f, -24f));
        }

        private void BuildMatchHero(MatchSummary match)
        {
            Image card = _ui.Panel(_content, "MatchHero", PulsePalette.Ink, true);
            VisualComposer.Size(card.gameObject, 370f);

            Text league = _ui.Label(card.transform, "League", $"{match.Region} · {match.League}".ToUpperInvariant(),
                22, PulsePalette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(league.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(30f, -62f), new Vector2(-30f, -18f));

            BuildBadge(card.transform, "HomeBadge", match.HomeBadgeUrl, new Vector2(0.18f, 0.58f), 106f);
            BuildBadge(card.transform, "AwayBadge", match.AwayBadgeUrl, new Vector2(0.82f, 0.58f), 106f);

            Text home = _ui.Label(card.transform, "Home", match.HomeTeam, 31, PulsePalette.White,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(home.rectTransform, Vector2.zero, new Vector2(0.42f, 0.42f),
                new Vector2(20f, 4f), new Vector2(-8f, 58f));
            Text away = _ui.Label(card.transform, "Away", match.AwayTeam, 31, PulsePalette.White,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(away.rectTransform, new Vector2(0.58f, 0f), new Vector2(1f, 0.42f),
                new Vector2(8f, 4f), new Vector2(-20f, 58f));

            DateTime local = match.KickoffUtc.ToLocalTime();
            Text time = _ui.Label(card.transform, "Kickoff", local.ToString("dd MMM\nHH:mm",
                    CultureInfo.GetCultureInfo("ru-RU")).ToUpperInvariant(),
                29, PulsePalette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(time.rectTransform, new Vector2(0.42f, 0.35f), new Vector2(0.58f, 0.76f),
                new Vector2(0f, 0f), Vector2.zero);

            Text venue = _ui.Label(card.transform, "Venue", match.Venue, 20,
                new Color(1f, 1f, 1f, 0.62f), TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(venue.rectTransform, Vector2.zero, new Vector2(1f, 0f),
                new Vector2(32f, 22f), new Vector2(-32f, 62f));
        }

        private void BuildTimeline(MatchTimelineEvent[] events)
        {
            BuildSectionTitle("ХРОНОЛОГИЯ МАТЧА", "ГОЛЫ · КАРТОЧКИ · ЗАМЕНЫ");
            if (events == null || events.Length == 0)
            {
                BuildNotice("СОБЫТИЙ ПОКА НЕТ", "Таймлайн появится после стартового свистка.");
                return;
            }

            int count = Math.Min(events.Length, 24);
            for (int index = 0; index < count; index++)
            {
                MatchTimelineEvent item = events[index];
                Image card = _ui.Panel(_content, "Event-" + index, PulsePalette.White, true);
                VisualComposer.Size(card.gameObject, 104f);

                string minute = item.Minute + (item.ExtraMinute > 0 ? "+" + item.ExtraMinute : string.Empty) + "′";
                Text time = _ui.Label(card.transform, "Minute", minute, 28, PulsePalette.Orange,
                    TextAnchor.MiddleCenter, FontStyle.Bold);
                VisualComposer.SetAnchors(time.rectTransform, Vector2.zero, new Vector2(0.14f, 1f),
                    new Vector2(10f, 6f), new Vector2(-4f, -6f));

                Text player = _ui.Label(card.transform, "Player",
                    string.IsNullOrWhiteSpace(item.Player) ? item.Team : item.Player,
                    25, PulsePalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
                VisualComposer.SetAnchors(player.rectTransform, new Vector2(0.14f, 0.42f), new Vector2(0.72f, 1f),
                    new Vector2(10f, 0f), new Vector2(-8f, -4f));

                string detail = LocalizeEvent(item);
                Text type = _ui.Label(card.transform, "Type", detail, 19, PulsePalette.Muted,
                    TextAnchor.MiddleLeft, FontStyle.Bold);
                VisualComposer.SetAnchors(type.rectTransform, new Vector2(0.14f, 0f), new Vector2(0.72f, 0.48f),
                    new Vector2(10f, 4f), new Vector2(-8f, 0f));

                Text team = _ui.Label(card.transform, "Team", item.Team, 20, PulsePalette.Muted,
                    TextAnchor.MiddleRight, FontStyle.Bold);
                VisualComposer.SetAnchors(team.rectTransform, new Vector2(0.72f, 0f), Vector2.one,
                    new Vector2(0f, 6f), new Vector2(-24f, -6f));
            }
        }

        private void BuildMetrics(MatchMetric[] metrics)
        {
            BuildSectionTitle("СТАТИСТИКА", "ХОЗЯЕВА · ГОСТИ");
            if (metrics == null || metrics.Length == 0)
            {
                BuildNotice("СТАТИСТИКА ЕЩЁ НЕ ГОТОВА", "Показатели появятся во время матча.");
                return;
            }

            Image card = _ui.Panel(_content, "Metrics", PulsePalette.White, true);
            float height = 42f + metrics.Length * 74f;
            VisualComposer.Size(card.gameObject, height);
            for (int index = 0; index < metrics.Length; index++)
            {
                MatchMetric metric = metrics[index];
                float top = -22f - index * 74f;
                Text home = _ui.Label(card.transform, "HomeValue", metric.HomeValue, 27,
                    PulsePalette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
                VisualComposer.SetAnchors(home.rectTransform, new Vector2(0f, 1f), new Vector2(0.22f, 1f),
                    new Vector2(12f, top - 58f), new Vector2(-4f, top));
                Text label = _ui.Label(card.transform, "Metric", metric.Label, 20,
                    PulsePalette.Muted, TextAnchor.MiddleCenter, FontStyle.Bold);
                VisualComposer.SetAnchors(label.rectTransform, new Vector2(0.22f, 1f), new Vector2(0.78f, 1f),
                    new Vector2(4f, top - 58f), new Vector2(-4f, top));
                Text away = _ui.Label(card.transform, "AwayValue", metric.AwayValue, 27,
                    PulsePalette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
                VisualComposer.SetAnchors(away.rectTransform, new Vector2(0.78f, 1f), Vector2.one,
                    new Vector2(4f, top - 58f), new Vector2(-12f, top));
            }
        }

        private void BuildLineups(MatchCenterLineup[] lineups)
        {
            BuildSectionTitle("СТАРТОВЫЕ СОСТАВЫ", "СХЕМА И НОМЕРА");
            if (lineups == null || lineups.Length < 2)
            {
                BuildNotice("СОСТАВЫ ЕЩЁ НЕ ОБЪЯВЛЕНЫ", "Обычно они появляются незадолго до начала матча.");
                return;
            }

            int playerCount = Math.Max(lineups[0].Starters.Length, lineups[1].Starters.Length);
            playerCount = Math.Min(playerCount, 11);
            Image card = _ui.Panel(_content, "Lineups", PulsePalette.White, true);
            VisualComposer.Size(card.gameObject, 124f + playerCount * 58f);
            BuildLineupHeader(card.transform, lineups[0], 0f, 0.5f, TextAnchor.MiddleLeft);
            BuildLineupHeader(card.transform, lineups[1], 0.5f, 1f, TextAnchor.MiddleRight);

            for (int index = 0; index < playerCount; index++)
            {
                float top = -106f - index * 58f;
                if (index < lineups[0].Starters.Length)
                    BuildPlayer(card.transform, lineups[0].Starters[index], 0f, 0.5f, top, TextAnchor.MiddleLeft);
                if (index < lineups[1].Starters.Length)
                    BuildPlayer(card.transform, lineups[1].Starters[index], 0.5f, 1f, top, TextAnchor.MiddleRight);
            }
        }

        private void BuildLineupHeader(Transform parent, MatchCenterLineup lineup, float min, float max,
            TextAnchor alignment)
        {
            Text team = _ui.Label(parent, "Team", lineup.Team, 24, PulsePalette.Ink, alignment, FontStyle.Bold);
            VisualComposer.SetAnchors(team.rectTransform, new Vector2(min, 1f), new Vector2(max, 1f),
                new Vector2(24f, -64f), new Vector2(-24f, -16f));
            Text formation = _ui.Label(parent, "Formation", "СХЕМА " + lineup.Formation, 18,
                PulsePalette.Orange, alignment, FontStyle.Bold);
            VisualComposer.SetAnchors(formation.rectTransform, new Vector2(min, 1f), new Vector2(max, 1f),
                new Vector2(24f, -102f), new Vector2(-24f, -64f));
        }

        private void BuildPlayer(Transform parent, LineupPlayer player, float min, float max, float top,
            TextAnchor alignment)
        {
            string value = alignment == TextAnchor.MiddleLeft
                ? $"{player.Number,2}  {player.Name}"
                : $"{player.Name}  {player.Number,2}";
            Text text = _ui.Label(parent, "Player", value, 20, PulsePalette.Ink, alignment, FontStyle.Bold);
            VisualComposer.SetAnchors(text.rectTransform, new Vector2(min, 1f), new Vector2(max, 1f),
                new Vector2(24f, top - 50f), new Vector2(-24f, top));
        }

        private void BuildSectionTitle(string titleValue, string badgeValue)
        {
            RectTransform row = _ui.FlowRow(_content, "Section", 72f);
            Text title = _ui.Label(row, "Title", titleValue, 29, PulsePalette.Ink,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.Size(title.gameObject, 72f, -1f, 1f);
            Text badge = _ui.Label(row, "Badge", badgeValue, 18, PulsePalette.Orange,
                TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.Size(badge.gameObject, 72f, 350f);
        }

        private void BuildNotice(string titleValue, string bodyValue, bool error = false)
        {
            Color background = error ? new Color(1f, 0.94f, 0.9f, 1f) : PulsePalette.White;
            Image card = _ui.Panel(_content, "Notice", background, true);
            VisualComposer.Size(card.gameObject, 150f);
            Text title = _ui.Label(card.transform, "Title", titleValue, 23,
                error ? PulsePalette.Danger : PulsePalette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, new Vector2(0f, 0.45f), Vector2.one,
                new Vector2(28f, 0f), new Vector2(-28f, -12f));
            Text body = _ui.Label(card.transform, "Body", bodyValue, 20, PulsePalette.Muted,
                TextAnchor.MiddleCenter);
            VisualComposer.SetAnchors(body.rectTransform, Vector2.zero, new Vector2(1f, 0.48f),
                new Vector2(28f, 10f), new Vector2(-28f, 0f));
        }

        private void BuildBadge(Transform parent, string name, string url, Vector2 anchor, float size)
        {
            float half = size * 0.5f;
            Image holder = _ui.Panel(parent, name, PulsePalette.White, true);
            VisualComposer.SetAnchors(holder.rectTransform, anchor, anchor,
                new Vector2(-half, -half), new Vector2(half, half));
            Image image = _ui.Panel(holder.transform, "Image", Color.clear);
            VisualComposer.SetAnchors(image.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(10f, 10f), new Vector2(-10f, -10f));
            _images.Load(image, url);
        }

        private static string LocalizeEvent(MatchTimelineEvent item)
        {
            if (item.Type == "Goal") return item.Detail == "Penalty" ? "ГОЛ · ПЕНАЛЬТИ" : "ГОЛ";
            if (item.Type == "Card") return item.Detail.Contains("Red") ? "КРАСНАЯ КАРТОЧКА" : "ЖЁЛТАЯ КАРТОЧКА";
            if (item.Type == "subst") return "ЗАМЕНА";
            if (item.Type == "Var") return "VAR";
            return string.IsNullOrWhiteSpace(item.Detail) ? item.Type.ToUpperInvariant() : item.Detail.ToUpperInvariant();
        }

        private void Clear()
        {
            for (int index = _content.childCount - 1; index >= 0; index--)
                UnityEngine.Object.Destroy(_content.GetChild(index).gameObject);
        }
    }
}
