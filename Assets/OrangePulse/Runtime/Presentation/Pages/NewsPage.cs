using System;
using System.Collections.Generic;
using OrangePulse.Core;
using OrangePulse.Data;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation.Pages
{
    public sealed class NewsPage : PageSurface
    {
        private readonly VisualComposer _ui;
        private readonly Action<NewsSection> _sectionChanged;
        private readonly Action<string> _openStory;
        private readonly Dictionary<NewsSection, Button> _filters = new();
        private readonly RectTransform _stories;
        private readonly Text _status;
        private NewsSection _section;

        public NewsPage(VisualComposer ui, Transform parent, Action<NewsSection> sectionChanged,
            Action<string> openStory)
        {
            _ui = ui;
            _sectionChanged = sectionChanged;
            _openStory = openStory;

            Root = ui.Panel(parent, "NewsPage", PulsePalette.Paper).rectTransform;
            VisualComposer.Stretch(Root);
            BuildHeader();
            BuildFilters();

            ScrollRect scroll = ui.VerticalScroll(Root, "NewsScroll", out _stories);
            VisualComposer.SetAnchors(scroll.GetComponent<RectTransform>(), Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(0f, -258f));

            _status = ui.Label(_stories, "Status", "ЗАГРУЖАЕМ СВЕЖИЕ МАТЕРИАЛЫ...", 24,
                PulsePalette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(_status.gameObject, 110f);
            SetSection(NewsSection.Football, false);
        }

        public void SetSection(NewsSection section, bool notify = true)
        {
            _section = section;
            foreach (KeyValuePair<NewsSection, Button> pair in _filters)
            {
                Image image = pair.Value.targetGraphic as Image;
                Text text = pair.Value.GetComponentInChildren<Text>();
                bool active = pair.Key == section;
                image.color = active ? PulsePalette.Orange : PulsePalette.White;
                text.color = active ? PulsePalette.Ink : PulsePalette.Muted;
            }
            if (notify) _sectionChanged?.Invoke(section);
        }

        public void ShowLoading()
        {
            Clear(_stories);
            Text loading = _ui.Label(_stories, "Status", "ОБНОВЛЯЕМ ЛЕНТУ...", 25,
                PulsePalette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(loading.gameObject, 140f);
        }

        public void Render(LoadResult<IReadOnlyList<NewsStory>> result)
        {
            Clear(_stories);
            if (result == null || !result.IsSuccess || result.Data == null)
            {
                BuildError(result?.Error ?? "Неизвестная ошибка");
                return;
            }

            Text state = _ui.Label(_stories, "State",
                result.FromCache ? "ОФЛАЙН-КОПИЯ · BBC SPORT" : "СВЕЖАЯ ЛЕНТА · BBC SPORT", 22,
                result.FromCache ? PulsePalette.Muted : PulsePalette.Success,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.Size(state.gameObject, 64f);

            foreach (NewsStory story in result.Data) BuildStory(story);

            Text credit = _ui.Label(_stories, "Credit",
                "ЗАГОЛОВКИ И ССЫЛКИ ПРЕДОСТАВЛЕНЫ BBC SPORT", 20,
                PulsePalette.Muted, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(credit.gameObject, 90f);
        }

        private void BuildHeader()
        {
            Image header = _ui.Panel(Root, "Header", PulsePalette.Ink);
            VisualComposer.SetAnchors(header.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -146f), Vector2.zero);

            Text title = _ui.Label(header.transform, "Title", "СПОРТИВНЫЙ ШУМ", 44,
                PulsePalette.White, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(42f, 34f), new Vector2(-40f, -24f));

            Text accent = _ui.Label(header.transform, "Accent", "●", 38, PulsePalette.Orange,
                TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.SetAnchors(accent.rectTransform, new Vector2(1f, 0f), Vector2.one,
                new Vector2(-110f, 32f), new Vector2(-42f, -28f));
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

            AddFilter(row, NewsSection.Football, "ФУТБОЛ");
            AddFilter(row, NewsSection.FormulaOne, "ФОРМУЛА-1");
            AddFilter(row, NewsSection.Tennis, "ТЕННИС");
        }

        private void AddFilter(Transform parent, NewsSection section, string label)
        {
            Button button = _ui.Button(parent, "Filter-" + section, label, PulsePalette.White,
                PulsePalette.Muted, 23, () => SetSection(section));
            _filters[section] = button;
        }

        private void BuildStory(NewsStory story)
        {
            Image card = _ui.Panel(_stories, "Story", PulsePalette.White, true);
            VisualComposer.Size(card.gameObject, 252f);
            Button action = card.gameObject.AddComponent<Button>();
            action.targetGraphic = card;
            action.onClick.AddListener(() => _openStory?.Invoke(story.Url));

            Text source = _ui.Label(card.transform, "Source", story.Source, 20,
                PulsePalette.Orange, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(source.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(28f, -56f), new Vector2(-300f, -16f));

            string date = story.PublishedUtc == default
                ? "СЕГОДНЯ"
                : story.PublishedUtc.ToLocalTime().ToString("dd.MM · HH:mm");
            Text published = _ui.Label(card.transform, "Published", date, 20,
                PulsePalette.Muted, TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.SetAnchors(published.rectTransform, new Vector2(1f, 1f), Vector2.one,
                new Vector2(-310f, -56f), new Vector2(-28f, -16f));

            Text title = _ui.Label(card.transform, "Title", story.Title, 34,
                PulsePalette.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(28f, 70f), new Vector2(-28f, -66f));

            Text hint = _ui.Label(card.transform, "Hint", "ЧИТАТЬ МАТЕРИАЛ  ↗", 22,
                PulsePalette.Orange, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(hint.rectTransform, Vector2.zero, new Vector2(1f, 0f),
                new Vector2(28f, 18f), new Vector2(-28f, 60f));
        }

        private void BuildError(string message)
        {
            Image card = _ui.Panel(_stories, "Error", new Color(1f, 0.94f, 0.9f, 1f), true);
            VisualComposer.Size(card.gameObject, 190f);
            Text text = _ui.Label(card.transform, "Text", "Лента временно недоступна\n" + message, 29,
                PulsePalette.Danger, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(text.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(26f, 20f), new Vector2(-26f, -20f));
        }

        private static void Clear(Transform parent)
        {
            for (int index = parent.childCount - 1; index >= 0; index--)
                UnityEngine.Object.Destroy(parent.GetChild(index).gameObject);
        }
    }
}

