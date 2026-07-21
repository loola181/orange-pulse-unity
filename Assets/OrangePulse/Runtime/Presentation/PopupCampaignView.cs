using System;
using OrangePulse.Data;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation
{
    public sealed class PopupCampaignView : MonoBehaviour
    {
        private static PopupCampaignView _active;
        private Campaign _campaign;
        private Action<string> _openUrl;
        private Sprite _artworkSprite;
        private bool _closing;

        public static PopupCampaignView Open(Campaign campaign, Texture2D artwork, Action<string> openUrl)
        {
            TryClose();
            var host = new GameObject("OrangeFootballPopup");
            _active = host.AddComponent<PopupCampaignView>();
            _active.Build(campaign, artwork, openUrl);
            return _active;
        }

        public static bool TryClose()
        {
            if (_active == null) return false;
            _active.Close();
            return true;
        }

        private void Build(Campaign campaign, Texture2D artwork, Action<string> openUrl)
        {
            _campaign = campaign;
            _openUrl = openUrl;
            var ui = new VisualComposer();
            Canvas canvas = ui.BuildCanvas();
            canvas.name = "PopupCanvas";
            canvas.sortingOrder = 300;
            canvas.transform.SetParent(transform, false);

            Image dim = ui.Panel(canvas.transform, "Dim", new Color(0f, 0f, 0f, 0.86f));
            VisualComposer.Stretch(dim.rectTransform);
            Button dismiss = dim.gameObject.AddComponent<Button>();
            dismiss.targetGraphic = dim;
            dismiss.onClick.AddListener(Close);

            Image card = ui.Panel(canvas.transform, "PopupCard", PulsePalette.Ink, true);
            VisualComposer.SetAnchors(card.rectTransform, new Vector2(0.055f, 0.13f),
                new Vector2(0.945f, 0.88f), Vector2.zero, Vector2.zero);
            Mask mask = card.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            Image artworkImage = ui.Panel(card.transform, "Artwork", PulsePalette.InkSoft);
            if (artwork != null)
            {
                _artworkSprite = VisualComposer.FromTexture(artwork, "PopupRemoteArtwork");
                artworkImage.sprite = _artworkSprite;
            }
            artworkImage.type = Image.Type.Simple;
            artworkImage.preserveAspect = false;

            string mode = campaign?.DisplayMode ?? "template";
            bool template = mode == "template";
            bool fullBanner = mode == "full_banner";
            VisualComposer.SetAnchors(artworkImage.rectTransform,
                template ? new Vector2(0f, 0.54f) : Vector2.zero,
                Vector2.one, Vector2.zero, Vector2.zero);

            Image shade = ui.Panel(card.transform, "Shade", template
                ? new Color(0f, 0f, 0f, 0.08f)
                : new Color(0f, 0f, 0f, fullBanner ? 0.12f : 0.64f));
            VisualComposer.Stretch(shade.rectTransform);

            if (fullBanner)
            {
                Button fullAction = card.gameObject.AddComponent<Button>();
                fullAction.targetGraphic = card;
                fullAction.onClick.AddListener(OpenAction);
            }
            else
            {
                BuildCopy(ui, card.transform, template);
            }

            Button close = ui.Button(card.transform, "Close", "×", new Color(0f, 0f, 0f, 0.72f),
                PulsePalette.White, 40, Close);
            VisualComposer.SetAnchors(close.GetComponent<RectTransform>(), new Vector2(1f, 1f), Vector2.one,
                new Vector2(-116f, -104f), new Vector2(-28f, -24f));
        }

        private void BuildCopy(VisualComposer ui, Transform card, bool template)
        {
            float lower = template ? 0f : 0.08f;
            float upper = template ? 0.54f : 0.92f;
            Image copy = ui.Panel(card, "Copy", template ? PulsePalette.Ink : Color.clear);
            VisualComposer.SetAnchors(copy.rectTransform, new Vector2(0f, lower), new Vector2(1f, upper),
                Vector2.zero, Vector2.zero);

            Text eyebrow = ui.Label(copy.transform, "Eyebrow", "ORANGE FOOTBALL · MATCHDAY", 23,
                PulsePalette.Orange, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(eyebrow.rectTransform, new Vector2(0f, 0.78f), Vector2.one,
                new Vector2(44f, 0f), new Vector2(-44f, 0f));

            Text title = ui.Label(copy.transform, "Title", _campaign.Title, 52,
                PulsePalette.White, TextAnchor.MiddleLeft, FontStyle.Bold);
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 30;
            title.resizeTextMaxSize = 52;
            VisualComposer.SetAnchors(title.rectTransform, new Vector2(0f, 0.48f), new Vector2(1f, 0.80f),
                new Vector2(44f, 0f), new Vector2(-44f, 0f));

            Text body = ui.Label(copy.transform, "Body", _campaign.Body, 27,
                new Color(1f, 1f, 1f, 0.76f), TextAnchor.UpperLeft);
            VisualComposer.SetAnchors(body.rectTransform, new Vector2(0f, 0.25f), new Vector2(1f, 0.49f),
                new Vector2(44f, 0f), new Vector2(-44f, 0f));

            Button action = ui.Button(copy.transform, "Action", _campaign.ButtonLabel, PulsePalette.Orange,
                PulsePalette.Ink, 27, OpenAction);
            VisualComposer.SetAnchors(action.GetComponent<RectTransform>(), new Vector2(0f, 0f),
                new Vector2(1f, 0f), new Vector2(44f, 34f), new Vector2(-44f, 122f));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        private void OpenAction()
        {
            if (_closing) return;
            string url = _campaign?.ButtonUrl;
            Close();
            _openUrl?.Invoke(url);
        }

        private void Close()
        {
            if (_closing) return;
            _closing = true;
            if (_active == this) _active = null;
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_artworkSprite != null) Destroy(_artworkSprite);
            if (_active == this) _active = null;
        }
    }
}
