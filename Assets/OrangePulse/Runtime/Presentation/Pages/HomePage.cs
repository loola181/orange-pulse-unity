using System;
using System.Collections.Generic;
using System.Globalization;
using OrangePulse.Core;
using OrangePulse.Data;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation.Pages
{
    public sealed class HomePage : PageSurface
    {
        private readonly VisualComposer _ui;
        private readonly Action _refresh;
        private readonly Action<string> _openExternal;
        private RectTransform _matchList;
        private Text _feedState;
        private Image _campaignImage;
        private GameObject _campaignCard;
        private Text _campaignEyebrow;
        private Text _campaignTitle;
        private Text _campaignBody;
        private Text _campaignButtonLabel;
        private string _campaignUrl;

        public HomePage(VisualComposer ui, Transform parent, Sprite localHero, Action refresh,
            Action<string> openExternal)
        {
            _ui = ui;
            _refresh = refresh;
            _openExternal = openExternal;

            Root = ui.Panel(parent, "PulsePage", PulsePalette.Paper).rectTransform;
            VisualComposer.Stretch(Root);

            BuildHeader();
            ScrollRect scroll = ui.VerticalScroll(Root, "PulseScroll", out RectTransform content);
            VisualComposer.SetAnchors(scroll.GetComponent<RectTransform>(), Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(0f, -146f));

            _campaignCard = BuildCampaign(content, localHero);
            BuildSectionHeading(content, out _feedState);
            _matchList = ui.Rect(content, "MatchList");
            VerticalLayoutGroup listLayout = _matchList.gameObject.AddComponent<VerticalLayoutGroup>();
            listLayout.spacing = 18f;
            listLayout.childControlHeight = true;
            listLayout.childControlWidth = true;
            listLayout.childForceExpandHeight = false;
            listLayout.childForceExpandWidth = true;
            ContentSizeFitter fitter = _matchList.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildFootnote(content);
            ShowMatchLoading();
        }

        public void SetCampaign(Campaign campaign)
        {
            bool show = campaign != null && campaign.Enabled;
            _campaignCard.SetActive(show);
            if (!show) return;

            _campaignEyebrow.text = string.IsNullOrWhiteSpace(campaign.Eyebrow)
                ? "ORANGE PULSE"
                : campaign.Eyebrow.ToUpperInvariant();
            _campaignTitle.text = campaign.Title;
            _campaignBody.text = campaign.Body;
            _campaignButtonLabel.text = campaign.ButtonLabel.ToUpperInvariant();
            _campaignUrl = campaign.ButtonUrl;
        }

        public void SetCampaignTexture(Texture2D texture)
        {
            if (texture == null) return;
            _campaignImage.sprite = VisualComposer.FromTexture(texture, "RemoteCampaign");
        }

        public void ShowMatchLoading()
        {
            _feedState.text = "ОБНОВЛЕНИЕ API";
            _feedState.color = PulsePalette.Orange;
            Clear(_matchList);
            Text placeholder = _ui.Label(_matchList, "Loading", "Собираем ближайшие события...", 31,
                PulsePalette.Muted, TextAnchor.MiddleCenter);
            VisualComposer.Size(placeholder.gameObject, 150f);
        }

        public void RenderMatches(LoadResult<IReadOnlyList<MatchSummary>> result)
        {
            Clear(_matchList);
            if (result == null || !result.IsSuccess || result.Data == null)
            {
                _feedState.text = "НЕТ СОЕДИНЕНИЯ";
                _feedState.color = PulsePalette.Danger;
                BuildError(result?.Error ?? "Не удалось получить данные");
                return;
            }

            _feedState.text = result.FromCache ? "СОХРАНЁННЫЕ ДАННЫЕ" : "ДАННЫЕ API · LIVE";
            _feedState.color = result.FromCache ? PulsePalette.Muted : PulsePalette.Success;
            foreach (MatchSummary match in result.Data) BuildMatch(match);
        }

        private void BuildHeader()
        {
            Image header = _ui.Panel(Root, "Header", PulsePalette.Ink);
            VisualComposer.SetAnchors(header.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -146f), Vector2.zero);

            Text brand = _ui.Label(header.transform, "Brand", "ORANGE", 42, PulsePalette.White,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(brand.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(42f, 42f), new Vector2(-520f, -22f));

            Text pulse = _ui.Label(header.transform, "Pulse", "PULSE", 42, PulsePalette.Orange,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(pulse.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(232f, 42f), new Vector2(-330f, -22f));

            Text caption = _ui.Label(header.transform, "Caption", "ТВОЙ СПОРТИВНЫЙ РИТМ", 22,
                new Color(1f, 1f, 1f, 0.58f), TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(caption.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(42f, 12f), new Vector2(-360f, -88f));

            Button refresh = _ui.Button(header.transform, "Refresh", "ОБНОВИТЬ", PulsePalette.Orange,
                PulsePalette.Ink, 23, _refresh);
            VisualComposer.SetAnchors(refresh.GetComponent<RectTransform>(), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(-254f, -34f), new Vector2(-42f, 34f));
        }

        private GameObject BuildCampaign(RectTransform content, Sprite localHero)
        {
            Image card = _ui.Panel(content, "CampaignCard", PulsePalette.Ink, true);
            VisualComposer.Size(card.gameObject, 430f);
            Mask mask = card.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            _campaignImage = _ui.Panel(card.transform, "Artwork", PulsePalette.InkSoft);
            _campaignImage.sprite = localHero;
            _campaignImage.type = Image.Type.Simple;
            VisualComposer.Stretch(_campaignImage.rectTransform);

            Image shade = _ui.Panel(card.transform, "Shade", new Color(0f, 0f, 0f, 0.62f));
            VisualComposer.Stretch(shade.rectTransform);

            _campaignEyebrow = _ui.Label(card.transform, "Eyebrow", "SERVER EDIT", 22,
                PulsePalette.Orange, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(_campaignEyebrow.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(34f, -92f), new Vector2(-34f, -36f));

            _campaignTitle = _ui.Label(card.transform, "Title", "Главный матч недели", 52,
                PulsePalette.White, TextAnchor.UpperLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(_campaignTitle.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(34f, -214f), new Vector2(-260f, -96f));

            _campaignBody = _ui.Label(card.transform, "Body", "Выбери событие и изучи детали до стартового свистка.",
                27, new Color(1f, 1f, 1f, 0.82f), TextAnchor.UpperLeft);
            VisualComposer.SetAnchors(_campaignBody.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(34f, -302f), new Vector2(-210f, -214f));

            Button cta = _ui.Button(card.transform, "CampaignAction", "ОТКРЫТЬ", PulsePalette.Orange,
                PulsePalette.Ink, 26, () => _openExternal?.Invoke(_campaignUrl));
            VisualComposer.SetAnchors(cta.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero,
                new Vector2(34f, 30f), new Vector2(330f, 104f));
            _campaignButtonLabel = cta.GetComponentInChildren<Text>();

            return card.gameObject;
        }

        private void BuildSectionHeading(RectTransform content, out Text state)
        {
            RectTransform heading = _ui.FlowRow(content, "ScheduleHeading", 74f);
            Text title = _ui.Label(heading, "Title", "БЛИЖАЙШИЕ СОБЫТИЯ", 34, PulsePalette.Ink,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.Size(title.gameObject, 74f, -1f, 1f);
            state = _ui.Label(heading, "State", "", 20, PulsePalette.Success,
                TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.Size(state.gameObject, 74f, 310f);
        }

        private void BuildMatch(MatchSummary match)
        {
            Image card = _ui.Panel(_matchList, "Match-" + match.Id, PulsePalette.White, true);
            VisualComposer.Size(card.gameObject, 210f);

            Image stripe = _ui.Panel(card.transform, "Stripe", PulsePalette.Orange, true);
            VisualComposer.SetAnchors(stripe.rectTransform, Vector2.zero, new Vector2(0f, 1f),
                new Vector2(0f, 18f), new Vector2(10f, -18f));

            Text region = _ui.Label(card.transform, "Region", match.Region, 21, PulsePalette.Orange,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(region.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(34f, -60f), new Vector2(-610f, -16f));

            Text league = _ui.Label(card.transform, "League", match.League.ToUpperInvariant(), 21,
                PulsePalette.Muted, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(league.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(116f, -60f), new Vector2(-250f, -16f));

            DateTime local = match.KickoffUtc.ToLocalTime();
            string day = local.ToString("dd MMM", CultureInfo.GetCultureInfo("ru-RU")).ToUpperInvariant();
            string time = local.ToString("HH:mm");
            Text kickoff = _ui.Label(card.transform, "Kickoff", day + "  ·  " + time, 25,
                PulsePalette.Ink, TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.SetAnchors(kickoff.rectTransform, new Vector2(1f, 1f), Vector2.one,
                new Vector2(-300f, -62f), new Vector2(-28f, -14f));

            Text home = _ui.Label(card.transform, "Home", match.HomeTeam, 34, PulsePalette.Ink,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(home.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(34f, 66f), new Vector2(-495f, -66f));

            Text versus = _ui.Label(card.transform, "Versus", "VS", 24, PulsePalette.Orange,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(versus.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f),
                new Vector2(-42f, 66f), new Vector2(42f, -66f));

            Text away = _ui.Label(card.transform, "Away", match.AwayTeam, 34, PulsePalette.Ink,
                TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.SetAnchors(away.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(495f, 66f), new Vector2(-28f, -66f));

            Text venue = _ui.Label(card.transform, "Venue", match.Venue, 22, PulsePalette.Muted,
                TextAnchor.MiddleLeft);
            VisualComposer.SetAnchors(venue.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(34f, 16f), new Vector2(-28f, -156f));
        }

        private void BuildError(string message)
        {
            Image card = _ui.Panel(_matchList, "Error", new Color(1f, 0.94f, 0.9f, 1f), true);
            VisualComposer.Size(card.gameObject, 170f);
            Text text = _ui.Label(card.transform, "Message", "Не удалось обновить ленту\n" + message, 28,
                PulsePalette.Danger, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(text.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(30f, 20f), new Vector2(-30f, -20f));
        }

        private void BuildFootnote(RectTransform content)
        {
            Text note = _ui.Label(content, "Footnote",
                "РАСПИСАНИЕ: THESPORTSDB  ·  ВРЕМЯ ПО ЧАСОВОМУ ПОЯСУ УСТРОЙСТВА", 20,
                PulsePalette.Muted, TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.Size(note.gameObject, 92f);
        }

        private static void Clear(Transform parent)
        {
            for (int index = parent.childCount - 1; index >= 0; index--)
                UnityEngine.Object.Destroy(parent.GetChild(index).gameObject);
        }
    }
}
