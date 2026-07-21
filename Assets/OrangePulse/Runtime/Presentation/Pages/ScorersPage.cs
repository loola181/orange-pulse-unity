using System;
using System.Collections.Generic;
using OrangePulse.Core;
using OrangePulse.Data;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation.Pages
{
    public sealed class ScorersPage : PageSurface
    {
        private readonly VisualComposer _ui;
        private readonly ClubBadgeLoader _images;
        private readonly Action<LeagueSource> _leagueChanged;
        private readonly Dictionary<string, Button> _filters = new();
        private readonly RectTransform _rows;
        private string _activeLeagueId;

        public ScorersPage(VisualComposer ui, Transform parent, ClubBadgeLoader images, Action back,
            Action<LeagueSource> leagueChanged)
        {
            _ui = ui;
            _images = images;
            _leagueChanged = leagueChanged;
            Root = ui.Panel(parent, "ScorersPage", PulsePalette.Paper).rectTransform;
            VisualComposer.Stretch(Root);
            BuildHeader(back);
            BuildFilters();

            ScrollRect scroll = ui.VerticalScroll(Root, "ScorersScroll", out _rows);
            VisualComposer.SetAnchors(scroll.GetComponent<RectTransform>(), Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(0f, -258f));
            SelectLeague(AppEndpoints.FeaturedLeagues[0].Id, false);
            ShowLoading();
        }

        public void SelectLeague(string leagueId, bool notify = true)
        {
            _activeLeagueId = leagueId;
            foreach (KeyValuePair<string, Button> filter in _filters)
            {
                bool active = filter.Key == leagueId;
                Image image = filter.Value.targetGraphic as Image;
                Text text = filter.Value.GetComponentInChildren<Text>();
                image.color = active ? PulsePalette.Orange : PulsePalette.White;
                text.color = active ? PulsePalette.Ink : PulsePalette.Muted;
            }

            if (!notify) return;
            foreach (LeagueSource league in AppEndpoints.FeaturedLeagues)
            {
                if (league.Id != leagueId) continue;
                _leagueChanged?.Invoke(league);
                return;
            }
        }

        public void ShowLoading()
        {
            Clear();
            Text loading = _ui.Label(_rows, "Loading", "СЧИТАЕМ ГОЛЫ И ПЕРЕДАЧИ...", 25,
                PulsePalette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(loading.gameObject, 150f);
        }

        public void Render(LeagueSource league, LoadResult<IReadOnlyList<ScorerRow>> result)
        {
            if (league.Id != _activeLeagueId) return;
            Clear();
            if (result == null || !result.IsSuccess || result.Data == null)
            {
                BuildError(result?.Error ?? "Не удалось получить список игроков");
                return;
            }

            Text status = _ui.Label(_rows, "Status",
                result.FromCache ? $"{league.Name} · СОХРАНЁННАЯ КОПИЯ" : $"{league.Name} · ТОП-20",
                21, result.FromCache ? PulsePalette.Muted : PulsePalette.Success,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.Size(status.gameObject, 60f);
            BuildColumns();
            foreach (ScorerRow row in result.Data) BuildRow(row);

            Text credit = _ui.Label(_rows, "Credit", "СТАТИСТИКА: API-FOOTBALL", 19,
                PulsePalette.Muted, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(credit.gameObject, 80f);
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

            Text title = _ui.Label(header.transform, "Title", "БОМБАРДИРЫ", 42,
                PulsePalette.White, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(210f, 30f), new Vector2(-235f, -24f));

            Text badge = _ui.Label(header.transform, "Badge", "ГОЛЕВАЯ ГОНКА", 19,
                PulsePalette.Orange, TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.SetAnchors(badge.rectTransform, new Vector2(1f, 0f), Vector2.one,
                new Vector2(-250f, 32f), new Vector2(-36f, -24f));
        }

        private void BuildFilters()
        {
            Image bar = _ui.Panel(Root, "Filters", PulsePalette.Paper);
            VisualComposer.SetAnchors(bar.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -258f), new Vector2(0f, -146f));
            RectTransform row = _ui.Rect(bar.transform, "Row");
            VisualComposer.SetAnchors(row, Vector2.zero, Vector2.one,
                new Vector2(40f, 20f), new Vector2(-40f, -20f));
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            AddFilter(row, AppEndpoints.FeaturedLeagues[0], "АПЛ");
            AddFilter(row, AppEndpoints.FeaturedLeagues[1], "ЛА ЛИГА");
            AddFilter(row, AppEndpoints.FeaturedLeagues[2], "БУНДЕС");
        }

        private void AddFilter(Transform parent, LeagueSource league, string label)
        {
            Button button = _ui.Button(parent, "League-" + league.Id, label, PulsePalette.White,
                PulsePalette.Muted, 22, () => SelectLeague(league.Id));
            _filters[league.Id] = button;
        }

        private void BuildColumns()
        {
            Image header = _ui.Panel(_rows, "Columns", PulsePalette.Ink, true);
            VisualComposer.Size(header.gameObject, 72f);
            AddCell(header.transform, "#", 0f, 0.08f, PulsePalette.Orange, TextAnchor.MiddleCenter, 22);
            AddCell(header.transform, "ИГРОК / КЛУБ", 0.17f, 0.68f, PulsePalette.White, TextAnchor.MiddleLeft, 20);
            AddCell(header.transform, "И", 0.68f, 0.78f, PulsePalette.Muted, TextAnchor.MiddleCenter, 20);
            AddCell(header.transform, "А", 0.78f, 0.89f, PulsePalette.Muted, TextAnchor.MiddleCenter, 20);
            AddCell(header.transform, "Г", 0.89f, 1f, PulsePalette.Orange, TextAnchor.MiddleCenter, 20);
        }

        private void BuildRow(ScorerRow row)
        {
            Color background = row.Rank <= 3 ? new Color(1f, 0.96f, 0.91f, 1f) : PulsePalette.White;
            Image card = _ui.Panel(_rows, "Scorer-" + row.Rank, background, true);
            VisualComposer.Size(card.gameObject, 142f);
            AddCell(card.transform, row.Rank.ToString(), 0f, 0.08f,
                row.Rank <= 3 ? PulsePalette.Orange : PulsePalette.Muted, TextAnchor.MiddleCenter, 28);

            Image portraitHolder = _ui.Panel(card.transform, "Portrait", PulsePalette.Paper, true);
            VisualComposer.SetAnchors(portraitHolder.rectTransform, new Vector2(0.08f, 0.5f),
                new Vector2(0.08f, 0.5f), new Vector2(8f, -45f), new Vector2(98f, 45f));
            Image portrait = _ui.Panel(portraitHolder.transform, "Image", Color.clear);
            VisualComposer.SetAnchors(portrait.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(5f, 5f), new Vector2(-5f, -5f));
            _images.Load(portrait, row.PlayerPhotoUrl);

            Text player = _ui.Label(card.transform, "Player", row.Player, 27, PulsePalette.Ink,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(player.rectTransform, new Vector2(0.08f, 0.5f), new Vector2(0.68f, 1f),
                new Vector2(112f, 0f), new Vector2(-8f, -8f));

            Image badge = _ui.Panel(card.transform, "ClubBadge", Color.clear);
            VisualComposer.SetAnchors(badge.rectTransform, new Vector2(0.08f, 0f), new Vector2(0.08f, 0.5f),
                new Vector2(112f, 17f), new Vector2(148f, -9f));
            _images.Load(badge, row.TeamBadgeUrl);

            Text team = _ui.Label(card.transform, "Team", row.Team, 20, PulsePalette.Muted,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(team.rectTransform, new Vector2(0.08f, 0f), new Vector2(0.68f, 0.5f),
                new Vector2(156f, 8f), new Vector2(-8f, -2f));

            AddCell(card.transform, row.Appearances.ToString(), 0.68f, 0.78f,
                PulsePalette.Muted, TextAnchor.MiddleCenter, 23);
            AddCell(card.transform, row.Assists.ToString(), 0.78f, 0.89f,
                PulsePalette.Success, TextAnchor.MiddleCenter, 24);
            AddCell(card.transform, row.Goals.ToString(), 0.89f, 1f,
                PulsePalette.Ink, TextAnchor.MiddleCenter, 31);
        }

        private void AddCell(Transform parent, string value, float min, float max, Color color,
            TextAnchor alignment, int size)
        {
            Text text = _ui.Label(parent, "Cell", value, size, color, alignment, FontStyle.Bold);
            VisualComposer.SetAnchors(text.rectTransform, new Vector2(min, 0f), new Vector2(max, 1f),
                new Vector2(6f, 4f), new Vector2(-6f, -4f));
        }

        private void BuildError(string message)
        {
            Image card = _ui.Panel(_rows, "Error", new Color(1f, 0.94f, 0.9f, 1f), true);
            VisualComposer.Size(card.gameObject, 190f);
            Text text = _ui.Label(card.transform, "Text", "Бомбардиры временно недоступны\n" + message,
                28, PulsePalette.Danger, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(text.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(28f, 20f), new Vector2(-28f, -20f));
        }

        private void Clear()
        {
            for (int index = _rows.childCount - 1; index >= 0; index--)
                UnityEngine.Object.Destroy(_rows.GetChild(index).gameObject);
        }
    }
}
