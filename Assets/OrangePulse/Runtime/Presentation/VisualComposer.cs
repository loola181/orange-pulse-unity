using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OrangePulse.Presentation
{
    public sealed class VisualComposer
    {
        private readonly Font _font;
        private readonly Sprite _rounded;

        public VisualComposer()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _rounded = BuildRoundedSprite();
        }

        public Canvas BuildCanvas()
        {
            var root = new GameObject("OrangePulseCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2340f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.55f;

            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            return canvas;
        }

        public RectTransform Rect(Transform parent, string name)
        {
            var value = new GameObject(name, typeof(RectTransform));
            value.transform.SetParent(parent, false);
            return value.GetComponent<RectTransform>();
        }

        public Image Panel(Transform parent, string name, Color color, bool rounded = false)
        {
            RectTransform rect = Rect(parent, name);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            if (rounded)
            {
                image.sprite = _rounded;
                image.type = Image.Type.Sliced;
            }
            return image;
        }

        public Text Label(Transform parent, string name, string value, int size, Color color,
            TextAnchor alignment = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            RectTransform rect = Rect(parent, name);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = _font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        public Button Button(Transform parent, string name, string label, Color background, Color foreground,
            int fontSize, Action clicked, bool rounded = true)
        {
            Image image = Panel(parent, name, background, rounded);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.86f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            button.colors = colors;
            if (clicked != null) button.onClick.AddListener(() => clicked());

            Text text = Label(button.transform, "Label", label, fontSize, foreground,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(text.rectTransform);
            return button;
        }

        public InputField TextField(Transform parent, string name, string value, string hint)
        {
            Image background = Panel(parent, name, PulsePalette.White, true);
            Outline outline = background.gameObject.AddComponent<Outline>();
            outline.effectColor = PulsePalette.Line;
            outline.effectDistance = new Vector2(1f, -1f);

            Text inputText = Label(background.transform, "Value", value, 34, PulsePalette.Ink);
            SetAnchors(inputText.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 8f), new Vector2(-28f, -8f));

            Text placeholder = Label(background.transform, "Hint", hint, 34,
                new Color(PulsePalette.Muted.r, PulsePalette.Muted.g, PulsePalette.Muted.b, 0.65f));
            SetAnchors(placeholder.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 8f), new Vector2(-28f, -8f));

            InputField field = background.gameObject.AddComponent<InputField>();
            field.textComponent = inputText;
            field.placeholder = placeholder;
            field.text = value;
            field.characterLimit = 24;
            field.lineType = InputField.LineType.SingleLine;
            field.contentType = InputField.ContentType.Standard;
            return field;
        }

        public ScrollRect VerticalScroll(Transform parent, string name, out RectTransform content)
        {
            Image root = Panel(parent, name, Color.clear);
            RectTransform viewport = Rect(root.transform, "Viewport");
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            content = Rect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 28, 36);
            layout.spacing = 22f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 44f;
            scroll.decelerationRate = 0.12f;
            return scroll;
        }

        public RectTransform FlowRow(Transform parent, string name, float height, float spacing = 16f)
        {
            RectTransform row = Rect(parent, name);
            LayoutElement element = row.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;
            return row;
        }

        public static LayoutElement Size(GameObject target, float preferredHeight, float preferredWidth = -1f,
            float flexibleWidth = -1f)
        {
            LayoutElement element = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
            element.preferredHeight = preferredHeight;
            if (preferredWidth >= 0f) element.preferredWidth = preferredWidth;
            if (flexibleWidth >= 0f) element.flexibleWidth = flexibleWidth;
            return element;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static Sprite FromTexture(Texture2D texture, string name)
        {
            if (texture == null) return null;
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f);
            sprite.name = name;
            return sprite;
        }

        public Sprite RoundedSprite => _rounded;

        private static Sprite BuildRoundedSprite()
        {
            const int size = 64;
            const float radius = 15f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "OrangePulseRounded",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nearestX = Mathf.Clamp(x, radius, size - 1f - radius);
                    float nearestY = Mathf.Clamp(y, radius, size - 1f - radius);
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(nearestX, nearestY));
                    byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((radius + 0.8f - distance) * 255f), 0, 255);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f,
                0, SpriteMeshType.FullRect, new Vector4(16f, 16f, 16f, 16f));
        }
    }
}

