using System;
using System.Collections.Generic;
using OrangePulse.Core;
using OrangePulse.Data;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation.Pages
{
    public sealed class StandingsPage : PageSurface
    {
        private readonly VisualComposer _ui;
        private readonly Action<LeagueSource> _leagueChanged;
        private readonly Dictionary<string, Button> _filters = new();
        private readonly RectTransform _rows;
        private string _activeLeagueId;

        public StandingsPage(VisualComposer ui, Transform parent, Action<LeagueSource> leagueChanged)
        {
            _ui = ui;
            _leagueChanged = leagueChanged;
            Root = ui.Panel(parent, "StandingsPage", PulsePalette.Paper).rectTransform;
            VisualComposer.Stretch(Root);
            BuildHeader();
            BuildFilters();

            ScrollRect scroll = ui.VerticalScroll(Root, "StandingsScroll", out _rows);
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
                if (league.Id == leagueId)
                {
                    _leagueChanged?.Invoke(league);
                    return;
                }
            }
        }

        public void ShowLoading()
        {
            Clear();
            Text loading = _ui.Label(_rows, "Loading", "СТРОИМ ТУРНИРНУЮ ТАБЛИЦУ...", 25,
                PulsePalette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(loading.gameObject, 150f);
        }

        public void Render(LeagueSource league, LoadResult<IReadOnlyList<StandingRow>> result)
        {
            if (league.Id != _activeLeagueId) return;
            Clear();
            if (result == null || !result.IsSuccess || result.Data == null)
            {
                BuildError(result?.Error ?? "Не удалось получить таблицу");
                return;
            }

            Text status = _ui.Label(_rows, "Status",
                result.FromCache ? $"{league.Name} · СОХРАНЁННАЯ КОПИЯ" : $"{league.Name} · API LIVE", 21,
                result.FromCache ? PulsePalette.Muted : PulsePalette.Success,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.Size(status.gameObject, 60f);
            BuildColumns();
            foreach (StandingRow row in result.Data) BuildRow(row);
        }

        private void BuildHeader()
        {
            Image header = _ui.Panel(Root, "Header", PulsePalette.Ink);
            VisualComposer.SetAnchors(header.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -146f), Vector2.zero);
            Text title = _ui.Label(header.transform, "Title", "ГОНКА ЗА ТИТУЛ", 44,
                PulsePalette.White, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(42f, 32f), new Vector2(-40f, -24f));
            Text badge = _ui.Label(header.transform, "Badge", "ВСЯ ЛИГА", 22,
                PulsePalette.Orange, TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.SetAnchors(badge.rectTransform, new Vector2(1f, 0f), Vector2.one,
                new Vector2(-220f, 32f), new Vector2(-42f, -24f));
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
            AddCell(header.transform, "#", 0f, 0.09f, TextAnchor.MiddleCenter, PulsePalette.Orange, 22);
            AddCell(header.transform, "КОМАНДА", 0.09f, 0.54f, TextAnchor.MiddleLeft, PulsePalette.White, 20);
            AddCell(header.transform, "И", 0.54f, 0.64f, TextAnchor.MiddleCenter, PulsePalette.Muted, 20);
            AddCell(header.transform, "В", 0.64f, 0.74f, TextAnchor.MiddleCenter, PulsePalette.Muted, 20);
            AddCell(header.transform, "Н", 0.74f, 0.84f, TextAnchor.MiddleCenter, PulsePalette.Muted, 20);
            AddCell(header.transform, "П", 0.84f, 0.92f, TextAnchor.MiddleCenter, PulsePalette.Muted, 20);
            AddCell(header.transform, "О", 0.92f, 1f, TextAnchor.MiddleCenter, PulsePalette.Orange, 20);
        }

        private void BuildRow(StandingRow row)
        {
            Color background = row.Rank <= 3 ? new Color(1f, 0.96f, 0.91f, 1f) : PulsePalette.White;
            Image card = _ui.Panel(_rows, "Standing-" + row.Rank, background, true);
            VisualComposer.Size(card.gameObject, 108f);
            AddCell(card.transform, row.Rank.ToString(), 0f, 0.09f, TextAnchor.MiddleCenter,
                row.Rank <= 3 ? PulsePalette.Orange : PulsePalette.Muted, 27);
            AddCell(card.transform, row.Team, 0.09f, 0.54f, TextAnchor.MiddleLeft, PulsePalette.Ink, 25);
            AddCell(card.transform, row.Played.ToString(), 0.54f, 0.64f, TextAnchor.MiddleCenter, PulsePalette.Muted, 22);
            AddCell(card.transform, row.Won.ToString(), 0.64f, 0.74f, TextAnchor.MiddleCenter, PulsePalette.Success, 22);
            AddCell(card.transform, row.Drawn.ToString(), 0.74f, 0.84f, TextAnchor.MiddleCenter, PulsePalette.Muted, 22);
            AddCell(card.transform, row.Lost.ToString(), 0.84f, 0.92f, TextAnchor.MiddleCenter, PulsePalette.Danger, 22);
            AddCell(card.transform, row.Points.ToString(), 0.92f, 1f, TextAnchor.MiddleCenter, PulsePalette.Ink, 25);
        }

        private void AddCell(Transform parent, string value, float min, float max,
            TextAnchor alignment, Color color, int size)
        {
            Text text = _ui.Label(parent, "Cell", value, size, color, alignment, FontStyle.Bold);
            VisualComposer.SetAnchors(text.rectTransform, new Vector2(min, 0f), new Vector2(max, 1f),
                new Vector2(8f, 4f), new Vector2(-8f, -4f));
        }

        private void BuildError(string message)
        {
            Image card = _ui.Panel(_rows, "Error", new Color(1f, 0.94f, 0.9f, 1f), true);
            VisualComposer.Size(card.gameObject, 190f);
            Text text = _ui.Label(card.transform, "Text", "Таблица временно недоступна\n" + message,
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
