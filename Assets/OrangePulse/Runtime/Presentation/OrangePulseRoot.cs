using System;
using System.Collections;
using System.Collections.Generic;
using OrangePulse.Core;
using OrangePulse.Data;
using OrangePulse.Native;
using OrangePulse.Presentation.Pages;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation
{
    public sealed class OrangePulseRoot : MonoBehaviour
    {
        private const float NavigationHeight = 136f;

        private enum AppTab
        {
            Pulse,
            Results,
            Standings,
            News,
            Profile
        }

        private readonly Dictionary<AppTab, PageSurface> _pages = new();
        private readonly Dictionary<AppTab, Button> _navigation = new();
        private readonly List<Texture2D> _downloadedTextures = new();

        private MatchFeedGateway _matches;
        private MatchResultsGateway _results;
        private StandingsGateway _standings;
        private TopScorersGateway _topScorers;
        private MatchCenterGateway _matchCenter;
        private NewsFeedGateway _news;
        private CampaignGateway _homeCampaign;
        private CampaignGateway _popupCampaign;
        private ProfileStore _profiles;
        private ImageVault _images;
        private GalleryBridge _gallery;
        private ProfileData _profile;
        private HomePage _homePage;
        private ResultsPage _resultsPage;
        private StandingsPage _standingsPage;
        private ScorersPage _scorersPage;
        private MatchCenterPage _matchCenterPage;
        private NewsPage _newsPage;
        private ProfilePage _profilePage;
        private HttpTransport _transport;
        private ClubBadgeLoader _clubBadges;
        private bool _matchesLoading;
        private bool _resultsLoading;
        private bool _standingsLoading;
        private bool _topScorersLoading;
        private bool _newsLoading;
        private LeagueSource _activeStandingsLeague = AppEndpoints.FeaturedLeagues[0];
        private LeagueSource _activeScorersLeague = AppEndpoints.FeaturedLeagues[0];
        private NewsSection _activeNews = NewsSection.Football;
        private string _activeMatchCenterId;
        private Sprite _heroSprite;
        private Sprite _iconSprite;

        private void Awake()
        {
            gameObject.name = "OrangePulseApp";
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;

            BuildServices();
            BuildInterface();
            RestoreProfile();
        }

        private void Start()
        {
            ShowTab(AppTab.Pulse);
            StartCoroutine(LoadRemoteCampaigns());
            StartCoroutine(LoadMatches());
            StartCoroutine(LoadResults());
            StartCoroutine(LoadStandings(_activeStandingsLeague));
            StartCoroutine(LoadNews(_activeNews));
        }

        private void OnDestroy()
        {
            _profiles?.Save(_profile);
            PopupCampaignView.TryClose();
            foreach (Texture2D texture in _downloadedTextures)
            {
                if (texture != null) Destroy(texture);
            }
            if (_heroSprite != null) Destroy(_heroSprite);
            if (_iconSprite != null) Destroy(_iconSprite);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) _profiles?.Save(_profile);
        }

        private void BuildServices()
        {
            _transport = new HttpTransport();
            _clubBadges = gameObject.AddComponent<ClubBadgeLoader>();
            _clubBadges.Initialize(_transport);
            var cache = new DiskTextCache();
            _matches = new MatchFeedGateway(_transport, cache);
            _results = new MatchResultsGateway(_transport, cache);
            _standings = new StandingsGateway(_transport, cache);
            _topScorers = new TopScorersGateway(_transport, cache);
            _matchCenter = new MatchCenterGateway(_transport, cache);
            _news = new NewsFeedGateway(_transport, cache);
            _homeCampaign = new CampaignGateway("home");
            _popupCampaign = new CampaignGateway("popup", false);
            _profiles = new ProfileStore();
            _images = new ImageVault();

            _gallery = gameObject.AddComponent<GalleryBridge>();
            _gallery.ImageSelected += OnImageSelected;
            _gallery.SelectionFailed += message => _profilePage?.ShowMessage(message, true);
        }

        private void BuildInterface()
        {
            var ui = new VisualComposer();
            Canvas canvas = ui.BuildCanvas();

            RectTransform safeArea = ui.Panel(canvas.transform, "SafeArea", PulsePalette.Paper).rectTransform;
            VisualComposer.Stretch(safeArea);
            safeArea.gameObject.AddComponent<SafeAreaContainer>();

            RectTransform pageHost = ui.Rect(safeArea, "PageHost");
            VisualComposer.SetAnchors(pageHost, Vector2.zero, Vector2.one,
                new Vector2(0f, NavigationHeight), Vector2.zero);

            Texture2D heroTexture = Resources.Load<Texture2D>("hero-stadium");
            Texture2D iconTexture = Resources.Load<Texture2D>("app-icon");
            _heroSprite = VisualComposer.FromTexture(heroTexture, "LocalHero");
            _iconSprite = VisualComposer.FromTexture(iconTexture, "OrangePulseIcon");

            _homePage = new HomePage(ui, pageHost, _heroSprite, _clubBadges, OnManualRefresh,
                OpenExternal, OpenMatchCenter);
            _resultsPage = new ResultsPage(ui, pageHost, _clubBadges, () => StartCoroutine(LoadResults()));
            _standingsPage = new StandingsPage(ui, pageHost, _clubBadges, OnStandingsLeagueChanged,
                OpenScorers);
            _newsPage = new NewsPage(ui, pageHost, OnNewsSectionChanged, OpenStory);
            _profilePage = new ProfilePage(ui, pageHost, _iconSprite, () => _gallery.Open(),
                () => OpenExternal(AppEndpoints.PrivacyPolicyUrl), SaveNickname, SaveFavoriteLeague);
            _matchCenterPage = new MatchCenterPage(ui, pageHost, _clubBadges, () => ShowTab(AppTab.Pulse));
            _scorersPage = new ScorersPage(ui, pageHost, _clubBadges, () => ShowTab(AppTab.Standings),
                OnScorersLeagueChanged);

            _pages[AppTab.Pulse] = _homePage;
            _pages[AppTab.Results] = _resultsPage;
            _pages[AppTab.Standings] = _standingsPage;
            _pages[AppTab.News] = _newsPage;
            _pages[AppTab.Profile] = _profilePage;
            _matchCenterPage.SetVisible(false);
            _scorersPage.SetVisible(false);
            BuildNavigation(ui, safeArea);
        }

        private void BuildNavigation(VisualComposer ui, RectTransform safeArea)
        {
            Image navigation = ui.Panel(safeArea, "Navigation", PulsePalette.Ink);
            VisualComposer.SetAnchors(navigation.rectTransform, Vector2.zero, new Vector2(1f, 0f),
                Vector2.zero, new Vector2(0f, NavigationHeight));

            RectTransform row = ui.Rect(navigation.transform, "NavigationRow");
            VisualComposer.SetAnchors(row, Vector2.zero, Vector2.one,
                new Vector2(16f, 12f), new Vector2(-16f, -12f));
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            AddNavigationButton(ui, row, AppTab.Pulse, "ГЛАВНАЯ");
            AddNavigationButton(ui, row, AppTab.Results, "ИТОГИ");
            AddNavigationButton(ui, row, AppTab.Standings, "ТАБЛИЦА");
            AddNavigationButton(ui, row, AppTab.News, "НОВОСТИ");
            AddNavigationButton(ui, row, AppTab.Profile, "ПРОФИЛЬ");
        }

        private void AddNavigationButton(VisualComposer ui, Transform parent, AppTab tab, string label)
        {
            Button button = ui.Button(parent, "Nav-" + tab, label, Color.clear,
                PulsePalette.White, 21, () => ShowTab(tab), false);
            _navigation[tab] = button;
        }

        private void ShowTab(AppTab tab)
        {
            _activeMatchCenterId = null;
            _matchCenterPage?.SetVisible(false);
            _scorersPage?.SetVisible(false);
            foreach (KeyValuePair<AppTab, PageSurface> page in _pages)
                page.Value.SetVisible(page.Key == tab);

            SetNavigationSelection(tab);
        }

        private void SetNavigationSelection(AppTab tab)
        {
            foreach (KeyValuePair<AppTab, Button> item in _navigation)
            {
                Text label = item.Value.GetComponentInChildren<Text>();
                label.color = item.Key == tab ? PulsePalette.Orange : PulsePalette.White;
            }
        }

        private void ShowOverlay(PageSurface overlay, AppTab navigationTab)
        {
            foreach (PageSurface page in _pages.Values) page.SetVisible(false);
            _matchCenterPage.SetVisible(ReferenceEquals(overlay, _matchCenterPage));
            _scorersPage.SetVisible(ReferenceEquals(overlay, _scorersPage));
            SetNavigationSelection(navigationTab);
        }

        private void RestoreProfile()
        {
            _profile = _profiles.Load();
            LeagueSource favoriteLeague = ResolveLeague(_profile.favoriteLeagueId);
            _activeStandingsLeague = favoriteLeague;
            _activeScorersLeague = favoriteLeague;
            _standingsPage.SelectLeague(favoriteLeague.Id, false);
            _scorersPage.SelectLeague(favoriteLeague.Id, false);
            _profilePage.SetProfile(_profile);
            if (_images.TryLoad(_profile.avatarPath, out Texture2D texture))
                _profilePage.SetAvatar(texture);
        }

        private void OnManualRefresh()
        {
            if (_matchesLoading) return;
            _profile.refreshedFeeds++;
            _profiles.Save(_profile);
            _profilePage.SetProfile(_profile);
            StartCoroutine(LoadMatches());
            StartCoroutine(LoadResults());
            StartCoroutine(LoadHomeCampaign());
        }

        private IEnumerator LoadMatches()
        {
            if (_matchesLoading) yield break;
            _matchesLoading = true;
            _homePage.ShowMatchLoading();

            LoadResult<IReadOnlyList<MatchSummary>> result = null;
            yield return _matches.LoadFeatured(value => result = value);
            _homePage.RenderMatches(result);
            _matchesLoading = false;
        }

        private IEnumerator LoadRemoteCampaigns()
        {
            yield return StartCoroutine(LoadHomeCampaign());
            yield return StartCoroutine(LoadPopupCampaign());
        }

        private IEnumerator LoadHomeCampaign()
        {
            _homePage.SetCampaign(FallbackCampaign());
            LoadResult<Campaign> result = null;
            yield return _homeCampaign.Load(value => result = value);
            if (result == null || !result.IsSuccess || result.Data == null) yield break;

            _homePage.SetCampaign(result.Data);
            if (result.Data.Enabled && !string.IsNullOrWhiteSpace(result.Data.ImageUrl))
            {
                LoadResult<Texture2D> image = null;
                yield return _transport.GetTexture(result.Data.ImageUrl, value => image = value);
                if (image != null && image.IsSuccess && image.Data != null)
                {
                    _downloadedTextures.Add(image.Data);
                    _homePage.SetCampaignTexture(image.Data);
                }
            }
        }

        private IEnumerator LoadPopupCampaign()
        {
            LoadResult<Campaign> result = null;
            yield return _popupCampaign.Load(value => result = value);
            if (result == null || !result.IsSuccess || result.Data == null || !result.Data.Enabled) yield break;

            Texture2D artwork = null;
            if (!string.IsNullOrWhiteSpace(result.Data.ImageUrl))
            {
                LoadResult<Texture2D> image = null;
                yield return _transport.GetTexture(result.Data.ImageUrl, value => image = value);
                if (image != null && image.IsSuccess && image.Data != null)
                {
                    artwork = image.Data;
                    _downloadedTextures.Add(artwork);
                }
            }
            PopupCampaignView.Open(result.Data, artwork, OpenExternal);
        }

        private IEnumerator LoadResults()
        {
            if (_resultsLoading) yield break;
            _resultsLoading = true;
            _resultsPage.ShowLoading();
            _homePage.ShowResultsLoading();
            LoadResult<IReadOnlyList<MatchResult>> result = null;
            yield return _results.LoadFeatured(value => result = value);
            _resultsPage.Render(result);
            _homePage.RenderRecentResults(result);
            _resultsLoading = false;
        }

        private void OnStandingsLeagueChanged(LeagueSource league)
        {
            _activeStandingsLeague = league;
            if (!_standingsLoading) StartCoroutine(LoadStandings(league));
        }

        private IEnumerator LoadStandings(LeagueSource league)
        {
            if (_standingsLoading) yield break;
            _standingsLoading = true;
            _standingsPage.ShowLoading();
            LoadResult<IReadOnlyList<StandingRow>> result = null;
            yield return _standings.Load(league, value => result = value);
            _standingsPage.Render(league, result);
            _standingsLoading = false;
            if (_activeStandingsLeague.Id != league.Id)
                StartCoroutine(LoadStandings(_activeStandingsLeague));
        }

        private void OpenScorers()
        {
            _profile.openedScorers++;
            SaveProfileActivity();
            _activeScorersLeague = _activeStandingsLeague;
            _scorersPage.SelectLeague(_activeScorersLeague.Id, false);
            ShowOverlay(_scorersPage, AppTab.Standings);
            if (!_topScorersLoading) StartCoroutine(LoadTopScorers(_activeScorersLeague));
        }

        private void OnScorersLeagueChanged(LeagueSource league)
        {
            _activeScorersLeague = league;
            if (!_topScorersLoading) StartCoroutine(LoadTopScorers(league));
        }

        private IEnumerator LoadTopScorers(LeagueSource league)
        {
            if (_topScorersLoading) yield break;
            _topScorersLoading = true;
            _scorersPage.ShowLoading();
            LoadResult<IReadOnlyList<ScorerRow>> result = null;
            yield return _topScorers.Load(league, value => result = value);
            _scorersPage.Render(league, result);
            _topScorersLoading = false;
            if (_activeScorersLeague.Id != league.Id)
                StartCoroutine(LoadTopScorers(_activeScorersLeague));
        }

        private void OpenMatchCenter(MatchSummary match)
        {
            if (match == null) return;
            _profile.openedMatchCenters++;
            SaveProfileActivity();
            _activeMatchCenterId = match.Id;
            ShowOverlay(_matchCenterPage, AppTab.Pulse);
            _matchCenterPage.ShowLoading(match);
            StartCoroutine(LoadMatchCenter(match));
        }

        private IEnumerator LoadMatchCenter(MatchSummary match)
        {
            LoadResult<MatchCenterData> result = null;
            yield return _matchCenter.Load(match.Id, value => result = value);
            if (_activeMatchCenterId == match.Id) _matchCenterPage.Render(match, result);
        }

        private void OnNewsSectionChanged(NewsSection section)
        {
            _activeNews = section;
            if (!_newsLoading) StartCoroutine(LoadNews(section));
        }

        private IEnumerator LoadNews(NewsSection section)
        {
            if (_newsLoading) yield break;
            _newsLoading = true;
            _newsPage.ShowLoading();

            LoadResult<IReadOnlyList<NewsStory>> result = null;
            yield return _news.Load(section, value => result = value);
            if (section == _activeNews) _newsPage.Render(result);
            _newsLoading = false;

            if (section != _activeNews) StartCoroutine(LoadNews(_activeNews));
        }

        private void OpenStory(string url)
        {
            if (!IsSafeHttps(url)) return;
            _profile.openedStories++;
            _profiles.Save(_profile);
            _profilePage.SetProfile(_profile);
            Application.OpenURL(url);
        }

        private static void OpenExternal(string url)
        {
            if (IsSafeHttps(url)) Application.OpenURL(url);
        }

        private void SaveNickname(string value)
        {
            _profile.nickname = ProfileStore.NormalizeName(value);
            _profiles.Save(_profile);
            _profilePage.SetProfile(_profile);
            _profilePage.ShowMessage("Профиль сохранён");
        }

        private void SaveFavoriteLeague(string leagueId)
        {
            _profile.favoriteLeagueId = ProfileStore.NormalizeLeagueId(leagueId);
            LeagueSource league = ResolveLeague(_profile.favoriteLeagueId);
            _activeStandingsLeague = league;
            _activeScorersLeague = league;
            _standingsPage.SelectLeague(league.Id, false);
            _scorersPage.SelectLeague(league.Id, false);
            _profiles.Save(_profile);
            _profilePage.SetProfile(_profile);
            _profilePage.ShowMessage("Любимая лига обновлена");
            if (!_standingsLoading) StartCoroutine(LoadStandings(league));
        }

        private void SaveProfileActivity()
        {
            _profiles.Save(_profile);
            _profilePage.SetProfile(_profile);
        }

        private static LeagueSource ResolveLeague(string leagueId)
        {
            string normalized = ProfileStore.NormalizeLeagueId(leagueId);
            foreach (LeagueSource league in AppEndpoints.FeaturedLeagues)
            {
                if (league.Id == normalized) return league;
            }
            return AppEndpoints.FeaturedLeagues[0];
        }

        private void OnImageSelected(string path)
        {
            if (!_images.TryImport(path, out Texture2D texture, out string savedPath, out string error))
            {
                _profilePage.ShowMessage(error, true);
                return;
            }

            _profile.avatarPath = savedPath;
            _profiles.Save(_profile);
            _profilePage.SetAvatar(texture);
            _profilePage.ShowMessage("Фото обновлено");
        }

        private static bool IsSafeHttps(string value) =>
            Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme == Uri.UriSchemeHttps;

        private static Campaign FallbackCampaign() => new()
        {
            Enabled = true,
            Eyebrow = "WEEKEND FOCUS",
            Title = "Главный матч недели",
            Body = "Расписание, состав участников и время старта — в одном касании.",
            ButtonLabel = "ОТКРЫТЬ ЛИГУ",
            ButtonUrl = "https://www.thesportsdb.com/league/4328-English-Premier-League",
            ImageUrl = string.Empty
        };
    }
}
