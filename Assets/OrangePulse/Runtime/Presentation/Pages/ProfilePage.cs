using System;
using System.Collections.Generic;
using OrangePulse.Core;
using OrangePulse.Data;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation.Pages
{
    public sealed class ProfilePage : PageSurface
    {
        private sealed class AchievementView
        {
            public Image Card;
            public Text Icon;
            public Text Body;
            public string Requirement;
        }

        private readonly VisualComposer _ui;
        private readonly Action _chooseImage;
        private readonly Action _openPrivacyPolicy;
        private readonly Action<string> _saveName;
        private readonly Action<string> _saveLeague;
        private readonly Dictionary<string, Button> _leagueButtons = new();
        private readonly AchievementView[] _achievements = new AchievementView[4];
        private readonly Image _avatar;
        private readonly AspectRatioFitter _avatarAspect;
        private readonly InputField _nickname;
        private readonly Text _toast;
        private readonly Text _heroName;
        private readonly Text _level;
        private readonly Text _levelTitle;
        private readonly Text _points;
        private readonly Image _progressFill;
        private readonly Text _storiesCount;
        private readonly Text _refreshCount;
        private readonly Text _matchesCount;
        private readonly Text _scorersCount;
        private readonly Text _achievementCount;
        private Sprite _runtimeAvatar;

        public ProfilePage(VisualComposer ui, Transform parent, Sprite defaultAvatar, Action chooseImage,
            Action openPrivacyPolicy, Action<string> saveName, Action<string> saveLeague)
        {
            _ui = ui;
            _chooseImage = chooseImage;
            _openPrivacyPolicy = openPrivacyPolicy;
            _saveName = saveName;
            _saveLeague = saveLeague;

            Root = ui.Panel(parent, "ProfilePage", PulsePalette.Paper).rectTransform;
            VisualComposer.Stretch(Root);
            BuildHeader();

            ScrollRect scroll = ui.VerticalScroll(Root, "ProfileScroll", out RectTransform content);
            VisualComposer.SetAnchors(scroll.GetComponent<RectTransform>(), Vector2.zero, Vector2.one,
                Vector2.zero, new Vector2(0f, -146f));

            BuildHero(content, defaultAvatar, out _avatar, out _avatarAspect, out _heroName, out _level,
                out _levelTitle, out _points, out _progressFill);
            BuildEditor(content, out _nickname, out _toast);
            BuildFavoriteLeague(content);
            BuildStats(content, out _matchesCount, out _scorersCount, out _storiesCount, out _refreshCount);
            BuildAchievements(content, out _achievementCount);
            BuildPrinciples(content);
        }

        public void SetProfile(ProfileData profile)
        {
            if (profile == null) return;
            string name = ProfileStore.NormalizeName(profile.nickname);
            int level = ProfileStore.Level(profile);
            int progress = ProfileStore.LevelProgress(profile);
            int unlocked = 0;

            _nickname.text = name;
            _heroName.text = name;
            _level.text = "УРОВЕНЬ " + level;
            _levelTitle.text = ProfileStore.LevelTitle(profile);
            _points.text = progress + " / 100 XP";
            _progressFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(progress / 100f), 1f);
            _storiesCount.text = profile.openedStories.ToString();
            _refreshCount.text = profile.refreshedFeeds.ToString();
            _matchesCount.text = profile.openedMatchCenters.ToString();
            _scorersCount.text = profile.openedScorers.ToString();
            SelectLeague(ProfileStore.NormalizeLeagueId(profile.favoriteLeagueId), false);

            unlocked += SetAchievement(0, profile.openedMatchCenters >= 1);
            unlocked += SetAchievement(1, profile.openedScorers >= 1);
            unlocked += SetAchievement(2, profile.openedStories >= 3);
            unlocked += SetAchievement(3, profile.refreshedFeeds >= 5);
            _achievementCount.text = unlocked + " / " + _achievements.Length + " ОТКРЫТО";
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
            Text title = _ui.Label(header.transform, "Title", "СПОРТИВНЫЙ ПРОФИЛЬ", 42,
                PulsePalette.White, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(42f, 30f), new Vector2(-300f, -24f));
            Text badge = _ui.Label(header.transform, "Badge", "ORANGE ID", 21,
                PulsePalette.Orange, TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.SetAnchors(badge.rectTransform, new Vector2(1f, 0f), Vector2.one,
                new Vector2(-260f, 32f), new Vector2(-42f, -24f));
        }

        private void BuildHero(RectTransform content, Sprite defaultAvatar, out Image avatar,
            out AspectRatioFitter avatarAspect, out Text heroName, out Text level, out Text levelTitle,
            out Text points, out Image progressFill)
        {
            Image card = _ui.Panel(content, "SportId", PulsePalette.Ink, true);
            VisualComposer.Size(card.gameObject, 410f);
            Image mark = _ui.Panel(card.transform, "OrangeMark", PulsePalette.Orange, true);
            VisualComposer.SetAnchors(mark.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -14f), Vector2.zero);

            Image avatarFrame = _ui.Panel(card.transform, "AvatarFrame", PulsePalette.White, true);
            VisualComposer.SetAnchors(avatarFrame.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(36f, -254f), new Vector2(238f, -52f));
            Mask mask = avatarFrame.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;
            avatar = _ui.Panel(avatarFrame.transform, "Avatar", PulsePalette.InkSoft);
            avatar.sprite = defaultAvatar;
            VisualComposer.Stretch(avatar.rectTransform);
            avatarAspect = avatar.gameObject.AddComponent<AspectRatioFitter>();
            avatarAspect.aspectRatio = 1f;
            avatarAspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            Button choose = _ui.Button(card.transform, "ChoosePhoto", "СМЕНИТЬ ФОТО", PulsePalette.White,
                PulsePalette.Ink, 19, _chooseImage);
            VisualComposer.SetAnchors(choose.GetComponent<RectTransform>(), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(36f, -336f), new Vector2(238f, -270f));

            level = _ui.Label(card.transform, "Level", "УРОВЕНЬ 1", 21, PulsePalette.Orange,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(level.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(276f, -92f), new Vector2(-32f, -44f));
            heroName = _ui.Label(card.transform, "Name", "Игрок 12", 42, PulsePalette.White,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(heroName.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(276f, -164f), new Vector2(-32f, -94f));
            levelTitle = _ui.Label(card.transform, "LevelTitle", "НОВИЧОК", 22,
                new Color(1f, 1f, 1f, 0.62f), TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(levelTitle.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(276f, -208f), new Vector2(-32f, -164f));

            Image track = _ui.Panel(card.transform, "ProgressTrack", new Color(1f, 1f, 1f, 0.15f), true);
            VisualComposer.SetAnchors(track.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(276f, -264f), new Vector2(-32f, -230f));
            progressFill = _ui.Panel(track.transform, "Progress", PulsePalette.Orange, true);
            VisualComposer.SetAnchors(progressFill.rectTransform, Vector2.zero, new Vector2(0f, 1f),
                Vector2.zero, Vector2.zero);
            points = _ui.Label(card.transform, "Points", "0 / 100 XP", 19,
                new Color(1f, 1f, 1f, 0.72f), TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.SetAnchors(points.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(276f, -310f), new Vector2(-32f, -270f));

            Text hint = _ui.Label(card.transform, "Hint",
                "Смотри матчи, читай новости и открывай новые уровни.", 20,
                new Color(1f, 1f, 1f, 0.58f), TextAnchor.MiddleLeft);
            VisualComposer.SetAnchors(hint.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(276f, 34f), new Vector2(-32f, -318f));
        }

        private void BuildEditor(RectTransform content, out InputField nickname, out Text toast)
        {
            Image card = _ui.Panel(content, "Editor", PulsePalette.White, true);
            VisualComposer.Size(card.gameObject, 255f);
            Text label = _ui.Label(card.transform, "NameLabel", "ИМЯ В ПРОФИЛЕ", 21,
                PulsePalette.Muted, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(label.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(32f, -62f), new Vector2(-32f, -18f));

            InputField createdNickname = _ui.TextField(card.transform, "Nickname", "Игрок 12", "Ваш ник");
            VisualComposer.SetAnchors(createdNickname.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one,
                new Vector2(32f, -150f), new Vector2(-230f, -72f));
            Button save = _ui.Button(card.transform, "SaveName", "СОХРАНИТЬ", PulsePalette.Orange,
                PulsePalette.Ink, 22, () => _saveName?.Invoke(createdNickname.text));
            VisualComposer.SetAnchors(save.GetComponent<RectTransform>(), new Vector2(1f, 1f), Vector2.one,
                new Vector2(-212f, -150f), new Vector2(-32f, -72f));

            toast = _ui.Label(card.transform, "Toast", "Данные профиля хранятся только на устройстве", 20,
                PulsePalette.Muted, TextAnchor.MiddleCenter);
            VisualComposer.SetAnchors(toast.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(28f, 22f), new Vector2(-28f, -176f));
            nickname = createdNickname;
        }

        private void BuildFavoriteLeague(RectTransform content)
        {
            Image card = _ui.Panel(content, "FavoriteLeague", new Color(1f, 0.93f, 0.85f, 1f), true);
            VisualComposer.Size(card.gameObject, 240f);
            Text title = _ui.Label(card.transform, "Title", "ЛЮБИМАЯ ЛИГА", 29,
                PulsePalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(30f, -64f), new Vector2(-30f, -18f));
            Text subtitle = _ui.Label(card.transform, "Subtitle",
                "Она будет открываться первой в таблицах и бомбардирах.", 20,
                PulsePalette.Muted, TextAnchor.MiddleLeft);
            VisualComposer.SetAnchors(subtitle.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(30f, -108f), new Vector2(-30f, -64f));

            RectTransform row = _ui.Rect(card.transform, "LeagueRow");
            VisualComposer.SetAnchors(row, Vector2.zero, Vector2.one,
                new Vector2(28f, 24f), new Vector2(-28f, -126f));
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            AddLeagueButton(row, AppEndpoints.FeaturedLeagues[0], "АПЛ");
            AddLeagueButton(row, AppEndpoints.FeaturedLeagues[1], "ЛА ЛИГА");
            AddLeagueButton(row, AppEndpoints.FeaturedLeagues[2], "БУНДЕС");
        }

        private void AddLeagueButton(Transform parent, LeagueSource league, string label)
        {
            Button button = _ui.Button(parent, "Favorite-" + league.Id, label, PulsePalette.White,
                PulsePalette.Muted, 20, () => SelectLeague(league.Id, true));
            _leagueButtons[league.Id] = button;
        }

        private void SelectLeague(string leagueId, bool notify)
        {
            foreach (KeyValuePair<string, Button> item in _leagueButtons)
            {
                bool active = item.Key == leagueId;
                Image background = item.Value.targetGraphic as Image;
                Text label = item.Value.GetComponentInChildren<Text>();
                background.color = active ? PulsePalette.Orange : PulsePalette.White;
                label.color = active ? PulsePalette.Ink : PulsePalette.Muted;
            }
            if (notify) _saveLeague?.Invoke(leagueId);
        }

        private void BuildStats(RectTransform content, out Text matches, out Text scorers,
            out Text stories, out Text refreshes)
        {
            Text title = _ui.Label(content, "StatsTitle", "ТВОЯ АКТИВНОСТЬ", 31,
                PulsePalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.Size(title.gameObject, 62f);

            RectTransform first = _ui.FlowRow(content, "StatsFirst", 160f, 16f);
            Image matchCard = StatCard(first, "МАТЧ-ЦЕНТР", out matches);
            VisualComposer.Size(matchCard.gameObject, 160f, -1f, 1f);
            Image scorerCard = StatCard(first, "БОМБАРДИРЫ", out scorers);
            VisualComposer.Size(scorerCard.gameObject, 160f, -1f, 1f);

            RectTransform second = _ui.FlowRow(content, "StatsSecond", 160f, 16f);
            Image storyCard = StatCard(second, "НОВОСТИ", out stories);
            VisualComposer.Size(storyCard.gameObject, 160f, -1f, 1f);
            Image refreshCard = StatCard(second, "ОБНОВЛЕНИЯ", out refreshes);
            VisualComposer.Size(refreshCard.gameObject, 160f, -1f, 1f);
        }

        private Image StatCard(Transform parent, string label, out Text value)
        {
            Image card = _ui.Panel(parent, "Stat-" + label, PulsePalette.Ink, true);
            value = _ui.Label(card.transform, "Value", "0", 54, PulsePalette.Orange,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(value.rectTransform, new Vector2(0f, 0.34f), Vector2.one,
                new Vector2(16f, 0f), new Vector2(-16f, -6f));
            Text caption = _ui.Label(card.transform, "Caption", label, 19, PulsePalette.White,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(caption.rectTransform, Vector2.zero, new Vector2(1f, 0.4f),
                new Vector2(12f, 10f), new Vector2(-12f, -2f));
            return card;
        }

        private void BuildAchievements(RectTransform content, out Text count)
        {
            RectTransform heading = _ui.FlowRow(content, "AchievementHeading", 66f);
            Text title = _ui.Label(heading, "Title", "ДОСТИЖЕНИЯ", 31,
                PulsePalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.Size(title.gameObject, 66f, -1f, 1f);
            count = _ui.Label(heading, "Count", "0 / 4 ОТКРЫТО", 19,
                PulsePalette.Orange, TextAnchor.MiddleRight, FontStyle.Bold);
            VisualComposer.Size(count.gameObject, 66f, 250f);

            Image board = _ui.Panel(content, "Achievements", PulsePalette.White, true);
            VisualComposer.Size(board.gameObject, 390f);
            _achievements[0] = BuildAchievement(board.transform, "ПЕРВЫЙ СВИСТОК",
                "Открой матч-центр", new Vector2(0f, 0.5f), new Vector2(0.5f, 1f));
            _achievements[1] = BuildAchievement(board.transform, "ГОЛЕВАЯ ГОНКА",
                "Посмотри бомбардиров", new Vector2(0.5f, 0.5f), Vector2.one);
            _achievements[2] = BuildAchievement(board.transform, "В КУРСЕ",
                "Прочитай 3 новости", Vector2.zero, new Vector2(0.5f, 0.5f));
            _achievements[3] = BuildAchievement(board.transform, "АНАЛИТИК",
                "Обнови ленту 5 раз", new Vector2(0.5f, 0f), new Vector2(1f, 0.5f));
        }

        private AchievementView BuildAchievement(Transform parent, string titleValue, string requirement,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            Image card = _ui.Panel(parent, "Achievement-" + titleValue, PulsePalette.Paper, true);
            Vector2 minOffset = new(anchorMin.x < 0.5f ? 16f : 8f, anchorMin.y < 0.5f ? 16f : 8f);
            Vector2 maxOffset = new(anchorMax.x > 0.5f ? -16f : -8f, anchorMax.y > 0.5f ? -16f : -8f);
            VisualComposer.SetAnchors(card.rectTransform, anchorMin, anchorMax, minOffset, maxOffset);

            Text icon = _ui.Label(card.transform, "Icon", "○", 38, PulsePalette.Muted,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            VisualComposer.SetAnchors(icon.rectTransform, new Vector2(0f, 0.48f), new Vector2(0.22f, 1f),
                new Vector2(8f, 0f), new Vector2(-2f, -8f));
            Text title = _ui.Label(card.transform, "Title", titleValue, 20, PulsePalette.Ink,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, new Vector2(0.22f, 0.48f), Vector2.one,
                new Vector2(2f, 0f), new Vector2(-12f, -8f));
            Text body = _ui.Label(card.transform, "Body", requirement, 18, PulsePalette.Muted,
                TextAnchor.UpperLeft);
            VisualComposer.SetAnchors(body.rectTransform, Vector2.zero, new Vector2(1f, 0.5f),
                new Vector2(18f, 10f), new Vector2(-12f, -4f));
            return new AchievementView { Card = card, Icon = icon, Body = body, Requirement = requirement };
        }

        private int SetAchievement(int index, bool unlocked)
        {
            AchievementView view = _achievements[index];
            view.Card.color = unlocked ? new Color(1f, 0.91f, 0.8f, 1f) : PulsePalette.Paper;
            view.Icon.text = unlocked ? "✓" : "○";
            view.Icon.color = unlocked ? PulsePalette.Success : PulsePalette.Muted;
            view.Body.text = unlocked ? "Достижение получено" : view.Requirement;
            view.Body.color = unlocked ? PulsePalette.Success : PulsePalette.Muted;
            return unlocked ? 1 : 0;
        }

        private void BuildPrinciples(RectTransform content)
        {
            Image card = _ui.Panel(content, "Principles", new Color(1f, 0.91f, 0.82f, 1f), true);
            VisualComposer.Size(card.gameObject, 310f);
            Text title = _ui.Label(card.transform, "Title", "СПОРТ БЕЗ ЛИШНЕГО РИСКА", 28,
                PulsePalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            VisualComposer.SetAnchors(title.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(30f, -68f), new Vector2(-30f, -18f));
            Text body = _ui.Label(card.transform, "Body",
                "Orange Football показывает спортивные данные и новости, но не принимает ставки и не обещает результат.",
                23, PulsePalette.InkSoft, TextAnchor.UpperLeft);
            VisualComposer.SetAnchors(body.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(30f, 112f), new Vector2(-30f, -82f));
            Button privacy = _ui.Button(card.transform, "PrivacyPolicy", "ПОЛИТИКА КОНФИДЕНЦИАЛЬНОСТИ",
                PulsePalette.Ink, PulsePalette.White, 20, _openPrivacyPolicy);
            VisualComposer.SetAnchors(privacy.GetComponent<RectTransform>(), Vector2.zero, new Vector2(1f, 0f),
                new Vector2(30f, 24f), new Vector2(-30f, 94f));
        }
    }
}
