using System;
using System.Collections.Generic;
using OrangePulse.Core;
using OrangePulse.Data;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation.Pages
{
    public sealed class ResultsPage : PageSurface
    {
        private readonly VisualComposer _ui;
        private readonly ClubBadgeLoader _clubBadges;
        private readonly RectTransform _list;

        public ResultsPage(VisualComposer ui, Transform parent, ClubBadgeLoader clubBadges, Action refresh)
        {
            _ui = ui;
            _clubBadges = clubBadges;
            Root = ui.Panel(parent, "ResultsPage", PulsePalette.Paper).rectTransform;
            VisualComposer.Stretch(Root);
            BuildHeader(refresh);

            ScrollRect scroll = ui.VerticalScroll(Root, "ResultsScroll", out _list);
            VisualComposer.SetAnchors(scroll.GetComponent<RectTransform>(), Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(0f, -146f));
            ShowLoading();
        }

        public void ShowLoading()
        {
            Clear();
            Text loading = _ui.Label(_list, "Loading", "ПРОВЕРЯЕМ ФИНАЛЬНЫЕ СВИСТКИ...", 25,
                PulsePalette.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(loading.gameObject, 150f);
        }

        public void Render(LoadResult<IReadOnlyList<MatchResult>> result)
        {
            Clear();
            if (result == null || !result.IsSuccess || result.Data == null)
            {
                BuildError(result?.Error ?? "Не удалось получить результаты");
                return;
            }

            Text status = _ui.Label(_list, "Status",
                result.FromCache ? "СОХРАНЁННЫЕ РЕЗУЛЬТАТЫ" : "ПОСЛЕДНИЕ РЕЗУЛЬТАТЫ · LIVE API", 22,
                result.FromCache ? PulsePalette.Muted : PulsePalette.Success,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.Size(status.gameObject, 64f);

            foreach (MatchResult match in result.Data) BuildResult(match);

            Text credit = _ui.Label(_list, "Credit", "РЕЗУЛЬТАТЫ: THESPORTSDB", 20,
                PulsePalette.Muted, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(credit.gameObject, 84f);
        }

        private void BuildHeader(Action refresh)
        {
            Image header = _ui.Panel(Root, "Header", PulsePalette.Ink);
            VisualComposer.SetAnchors(header.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -146f), Vector2.zero);

            Text title = _ui.Label(header.transform, "Title", "ФИНАЛЬНЫЙ СЧЁТ", 43,
                PulsePalette.White, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(42f, 30f), new Vector2(-360f, -24f));

            Button button = _ui.Button(header.transform, "Refresh", "ОБНОВИТЬ", PulsePalette.Orange,
                PulsePalette.Ink, 22, refresh);
            VisualComposer.SetAnchors(button.GetComponent<RectTransform>(), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(-250f, -34f), new Vector2(-42f, 34f));
        }

        private void BuildResult(MatchResult match)
        {
            Image card = _ui.Panel(_list, "Result-" + match.Id, PulsePalette.White, true);
            VisualComposer.Size(card.gameObject, 244f);

            Image stripe = _ui.Panel(card.transform, "Stripe", PulsePalette.Orange, true);
            VisualComposer.SetAnchors(stripe.rectTransform, Vector2.zero, new Vector2(0f, 1f),
                new Vector2(0f, 18f), new Vector2(10f, -18f));

            Text league = _ui.Label(card.transform, "League", $"{match.Region}  ·  {match.League}", 20,
                PulsePalette.Orange, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(league.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(30f, -58f), new Vector2(-300f, -16f));

            string date = match.PlayedUtc.ToLocalTime().ToString("dd MMM").ToUpperInvariant();
            Text state = _ui.Label(card.transform, "State", $"{date}  ·  {match.Status}", 20,
                PulsePalette.Muted, TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.SetAnchors(state.rectTransform, new Vector2(1f, 1f), Vector2.one,
                new Vector2(-360f, -58f), new Vector2(-28f, -16f));

            BuildBadge(card.transform, "HomeBadge", match.HomeBadgeUrl,
                new Vector2(28f, 108f), new Vector2(82f, 162f));
            BuildBadge(card.transform, "AwayBadge", match.AwayBadgeUrl,
                new Vector2(28f, 40f), new Vector2(82f, 94f));

            Text home = _ui.Label(card.transform, "Home", match.HomeTeam, 31,
                PulsePalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(home.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(94f, 92f), new Vector2(-330f, -70f));

            Text away = _ui.Label(card.transform, "Away", match.AwayTeam, 31,
                PulsePalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(away.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(94f, 24f), new Vector2(-330f, -138f));

            Text score = _ui.Label(card.transform, "Score", $"{match.HomeScore}\n{match.AwayScore}", 40,
                PulsePalette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(score.rectTransform, new Vector2(1f, 0f), Vector2.one,
                new Vector2(-270f, 28f), new Vector2(-34f, -72f));
        }

        private void BuildBadge(Transform parent, string name, string url, Vector2 offsetMin,
            Vector2 offsetMax)
        {
            Image holder = _ui.Panel(parent, name, PulsePalette.Paper, true);
            holder.raycastTarget = false;
            VisualComposer.SetAnchors(holder.rectTransform, Vector2.zero, Vector2.zero,
                offsetMin, offsetMax);
            Image logo = _ui.Panel(holder.transform, "Logo", Color.clear);
            VisualComposer.SetAnchors(logo.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(7f, 7f), new Vector2(-7f, -7f));
            _clubBadges.Load(logo, url);
        }

        private void BuildError(string message)
        {
            Image card = _ui.Panel(_list, "Error", new Color(1f, 0.94f, 0.9f, 1f), true);
            VisualComposer.Size(card.gameObject, 190f);
            Text text = _ui.Label(card.transform, "Text", "Результаты временно недоступны\n" + message,
                28, PulsePalette.Danger, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(text.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(28f, 20f), new Vector2(-28f, -20f));
        }

        private void Clear()
        {
            for (int index = _list.childCount - 1; index >= 0; index--)
                UnityEngine.Object.Destroy(_list.GetChild(index).gameObject);
        }
    }
}
