using System;
using OrangePulse.Data;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation.Pages
{
    public sealed class ProfilePage : PageSurface
    {
        private readonly VisualComposer _ui;
        private readonly Action _chooseImage;
        private readonly Action<string> _saveName;
        private readonly Image _avatar;
        private readonly AspectRatioFitter _avatarAspect;
        private readonly InputField _nickname;
        private readonly Text _toast;
        private readonly Text _storiesCount;
        private readonly Text _refreshCount;
        private Sprite _runtimeAvatar;

        public ProfilePage(VisualComposer ui, Transform parent, Sprite defaultAvatar, Action chooseImage,
            Action<string> saveName)
        {
            _ui = ui;
            _chooseImage = chooseImage;
            _saveName = saveName;

            Root = ui.Panel(parent, "ProfilePage", PulsePalette.Paper).rectTransform;
            VisualComposer.Stretch(Root);
            BuildHeader();

            ScrollRect scroll = ui.VerticalScroll(Root, "ProfileScroll", out RectTransform content);
            VisualComposer.SetAnchors(scroll.GetComponent<RectTransform>(), Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(0f, -146f));

            BuildIdentityCard(content, defaultAvatar, out _avatar, out _avatarAspect, out _nickname, out _toast);
            BuildStats(content, out _storiesCount, out _refreshCount);
            BuildPrinciples(content);
        }

        public void SetProfile(ProfileData profile)
        {
            if (profile == null) return;
            _nickname.text = ProfileStore.NormalizeName(profile.nickname);
            _storiesCount.text = profile.openedStories.ToString();
            _refreshCount.text = profile.refreshedFeeds.ToString();
        }

        public void SetAvatar(Texture2D texture)
        {
            if (texture == null) return;
            if (_runtimeAvatar != null) UnityEngine.Object.Destroy(_runtimeAvatar);
            _runtimeAvatar = VisualComposer.FromTexture(texture, "ProfileAvatar");
            _avatar.sprite = _runtimeAvatar;
            _avatarAspect.aspectRatio = texture.width / (float)Mathf.Max(1, texture.height);
            _avatarAspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }

        public void ShowMessage(string message, bool error = false)
        {
            _toast.text = message;
            _toast.color = error ? PulsePalette.Danger : PulsePalette.Success;
        }

        private void BuildHeader()
        {
            Image header = _ui.Panel(Root, "Header", PulsePalette.Ink);
            VisualComposer.SetAnchors(header.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -146f), Vector2.zero);

            Text title = _ui.Label(header.transform, "Title", "МОЙ ORANGE ID", 44,
                PulsePalette.White, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(42f, 30f), new Vector2(-42f, -24f));
        }

        private void BuildIdentityCard(RectTransform content, Sprite defaultAvatar, out Image avatar,
            out AspectRatioFitter avatarAspect, out InputField nickname, out Text toast)
        {
            Image card = _ui.Panel(content, "Identity", PulsePalette.White, true);
            VisualComposer.Size(card.gameObject, 650f);

            Image mark = _ui.Panel(card.transform, "OrangeMark", PulsePalette.Orange, true);
            VisualComposer.SetAnchors(mark.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -14f), Vector2.zero);

            Image avatarFrame = _ui.Panel(card.transform, "AvatarFrame", PulsePalette.Ink, true);
            avatarFrame.sprite = _ui.RoundedSprite;
            VisualComposer.SetAnchors(avatarFrame.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-132f, -302f), new Vector2(132f, -38f));
            Mask mask = avatarFrame.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            avatar = _ui.Panel(avatarFrame.transform, "Avatar", PulsePalette.InkSoft);
            avatar.sprite = defaultAvatar;
            VisualComposer.Stretch(avatar.rectTransform);
            avatarAspect = avatar.gameObject.AddComponent<AspectRatioFitter>();
            avatarAspect.aspectRatio = 1f;
            avatarAspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            Button choose = _ui.Button(card.transform, "ChoosePhoto", "ВЫБРАТЬ ФОТО ИЗ ГАЛЕРЕИ",
                PulsePalette.Ink, PulsePalette.White, 25, _chooseImage);
            VisualComposer.SetAnchors(choose.GetComponent<RectTransform>(), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(-330f, -390f), new Vector2(330f, -320f));

            Text label = _ui.Label(card.transform, "NameLabel", "КАК К ТЕБЕ ОБРАЩАТЬСЯ", 21,
                PulsePalette.Muted, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(label.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(38f, -452f), new Vector2(-38f, -404f));

            InputField createdNickname = _ui.TextField(card.transform, "Nickname", "Игрок 12", "Ваш ник");
            VisualComposer.SetAnchors(createdNickname.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one,
                new Vector2(38f, -542f), new Vector2(-238f, -462f));

            Button save = _ui.Button(card.transform, "SaveName", "СОХРАНИТЬ", PulsePalette.Orange,
                PulsePalette.Ink, 24, () => _saveName?.Invoke(createdNickname.text));
            VisualComposer.SetAnchors(save.GetComponent<RectTransform>(), new Vector2(1f, 1f), Vector2.one,
                new Vector2(-220f, -542f), new Vector2(-38f, -462f));

            toast = _ui.Label(card.transform, "Toast", "Профиль хранится только на устройстве", 23,
                PulsePalette.Muted, TextAnchor.MiddleCenter);
            VisualComposer.SetAnchors(toast.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(30f, 20f), new Vector2(-30f, -566f));
            nickname = createdNickname;
        }

        private void BuildStats(RectTransform content, out Text stories, out Text refreshes)
        {
            Text title = _ui.Label(content, "StatsTitle", "ТВОЯ АКТИВНОСТЬ", 32,
                PulsePalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.Size(title.gameObject, 64f);

            RectTransform row = _ui.FlowRow(content, "Stats", 190f, 18f);
            Image storiesCard = StatCard(row, "ПРОЧИТАНО", out stories);
            VisualComposer.Size(storiesCard.gameObject, 190f, -1f, 1f);
            Image refreshCard = StatCard(row, "ОБНОВЛЕНИЙ", out refreshes);
            VisualComposer.Size(refreshCard.gameObject, 190f, -1f, 1f);
        }

        private Image StatCard(Transform parent, string label, out Text value)
        {
            Image card = _ui.Panel(parent, "Stat-" + label, PulsePalette.Ink, true);
            value = _ui.Label(card.transform, "Value", "0", 66, PulsePalette.Orange,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(value.rectTransform, new Vector2(0f, 0.35f), Vector2.one,
                new Vector2(20f, 0f), new Vector2(-20f, -10f));
            Text caption = _ui.Label(card.transform, "Caption", label, 21, PulsePalette.White,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(caption.rectTransform, Vector2.zero, new Vector2(1f, 0.4f),
                new Vector2(20f, 12f), new Vector2(-20f, -4f));
            return card;
        }

        private void BuildPrinciples(RectTransform content)
        {
            Image card = _ui.Panel(content, "Principles", new Color(1f, 0.91f, 0.82f, 1f), true);
            VisualComposer.Size(card.gameObject, 250f);

            Text title = _ui.Label(card.transform, "Title", "СПОРТ БЕЗ ЛИШНЕГО РИСКА", 30,
                PulsePalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(32f, -78f), new Vector2(-32f, -22f));

            Text body = _ui.Label(card.transform, "Body",
                "Orange Football показывает расписание и новости. Приложение не принимает ставки и не обещает результат.",
                27, PulsePalette.InkSoft, TextAnchor.UpperLeft);
            VisualComposer.SetAnchors(body.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(32f, 24f), new Vector2(-32f, -92f));
        }
    }
}
