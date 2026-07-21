using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.RemoteConfig;
using OrangePulse.Core;
using UnityEngine;

namespace OrangePulse.Data
{
    public sealed class CampaignGateway
    {
        public const string EnabledKey = "orange_football_home_promo_enabled";
        public const string LaunchEnabledKey = "orange_football_home_promo_launch_enabled";
        public const string ClickActionKey = "orange_football_home_promo_click_action";
        public const string ClickUrlKey = "orange_football_home_promo_click_url";
        public const string ImageUrlKey = "orange_football_home_promo_image_url";
        public const string ImageRevisionKey = "orange_football_home_promo_image_revision";
        public const string TitleKey = "orange_football_home_promo_title";
        public const string SubtitleKey = "orange_football_home_promo_subtitle";
        public const string DisplayModeKey = "orange_football_home_promo_display_mode";

        private const string LeagueUrl =
            "https://www.thesportsdb.com/league/4328-English-Premier-League";

        private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(4);
        private readonly string _parameterPrefix;
        private readonly bool _defaultEnabled;

        public CampaignGateway(string channelKey = "home", bool defaultEnabled = true)
        {
            if (string.IsNullOrWhiteSpace(channelKey)) throw new ArgumentException("channelKey is required");
            foreach (char symbol in channelKey)
            {
                if (!char.IsLetterOrDigit(symbol) && symbol != '_')
                    throw new ArgumentException("channelKey contains unsupported symbols");
            }
            _parameterPrefix = $"orange_football_{channelKey.ToLowerInvariant()}_";
            _defaultEnabled = defaultEnabled;
        }

        public IEnumerator Load(Action<LoadResult<Campaign>> finished)
        {
            Task<Campaign> operation = FetchCampaign();
            while (!operation.IsCompleted) yield return null;

            if (operation.IsCanceled)
            {
                finished?.Invoke(LoadResult<Campaign>.Failed("Загрузка баннера отменена"));
                yield break;
            }

            if (operation.IsFaulted)
            {
                string message = operation.Exception?.GetBaseException().Message ?? "Баннер недоступен";
                Debug.LogWarning($"[OrangeFootball] Remote Config unavailable: {message}");
                finished?.Invoke(LoadResult<Campaign>.Failed(message));
                yield break;
            }

            finished?.Invoke(LoadResult<Campaign>.Fresh(operation.Result));
        }

        public static Campaign MapValues(
            bool enabled,
            bool launchEnabled,
            string clickAction,
            string clickUrl,
            string imageUrl,
            string title,
            string subtitle,
            string imageRevision = "builtin-v1",
            string displayMode = "background")
        {
            string action = (clickAction ?? string.Empty).Trim().ToLowerInvariant();
            string safeActionUrl = action == "url" && IsSafeHttps(clickUrl) ? clickUrl : LeagueUrl;
            string safeImageUrl = IsSafeHttps(imageUrl) ? imageUrl : string.Empty;

            return new Campaign
            {
                Enabled = enabled && launchEnabled,
                Eyebrow = "ГЛАВНОЕ · СЕГОДНЯ",
                Title = string.IsNullOrWhiteSpace(title) ? "Главный матч недели" : title.Trim(),
                Body = string.IsNullOrWhiteSpace(subtitle)
                    ? "Расписание и новости футбола в одном приложении."
                    : subtitle.Trim(),
                ButtonLabel = action == "url" ? "ОТКРЫТЬ" : "СМОТРЕТЬ МАТЧИ",
                ButtonUrl = safeActionUrl,
                ImageUrl = safeImageUrl,
                ImageRevision = string.IsNullOrWhiteSpace(imageRevision) ? "builtin-v1" : imageRevision.Trim(),
                DisplayMode = NormalizeDisplayMode(displayMode, safeImageUrl)
            };
        }

        private async Task<Campaign> FetchCampaign()
        {
            DependencyStatus dependencies = await CompleteWithin(
                FirebaseApp.CheckAndFixDependenciesAsync());
            if (dependencies != DependencyStatus.Available)
                throw new InvalidOperationException($"Firebase dependencies: {dependencies}");

            FirebaseRemoteConfig config = FirebaseRemoteConfig.DefaultInstance;
            await CompleteWithin(config.SetConfigSettingsAsync(new ConfigSettings
            {
                FetchTimeoutInMilliseconds = (ulong)OperationTimeout.TotalMilliseconds,
                MinimumFetchIntervalInMilliseconds = 0
            }));

            await CompleteWithin(config.SetDefaultsAsync(DefaultValues()));
            try
            {
                await CompleteWithin(config.FetchAsync(TimeSpan.Zero));
                await CompleteWithin(config.ActivateAsync());
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangeFootball] Using cached banner values: {exception.Message}");
            }

            return MapValues(
                config.GetValue(Key("promo_enabled")).BooleanValue,
                config.GetValue(Key("promo_launch_enabled")).BooleanValue,
                config.GetValue(Key("promo_click_action")).StringValue,
                config.GetValue(Key("promo_click_url")).StringValue,
                config.GetValue(Key("promo_image_url")).StringValue,
                config.GetValue(Key("promo_title")).StringValue,
                config.GetValue(Key("promo_subtitle")).StringValue,
                config.GetValue(Key("promo_image_revision")).StringValue,
                config.GetValue(Key("promo_display_mode")).StringValue);
        }

        private Dictionary<string, object> DefaultValues() => new()
        {
            { Key("promo_enabled"), _defaultEnabled },
            { Key("promo_launch_enabled"), _defaultEnabled },
            { Key("promo_click_action"), "url" },
            { Key("promo_click_url"), LeagueUrl },
            { Key("promo_image_url"), string.Empty },
            { Key("promo_image_revision"), "builtin-v1" },
            { Key("promo_title"), "Главный матч недели" },
            { Key("promo_subtitle"), "Расписание и новости футбола в одном приложении." },
            { Key("promo_display_mode"), "background" }
        };

        private string Key(string suffix) => _parameterPrefix + suffix;

        private static async Task CompleteWithin(Task operation)
        {
            Task winner = await Task.WhenAny(operation, Task.Delay(OperationTimeout));
            if (!ReferenceEquals(winner, operation)) throw new TimeoutException("Firebase request timed out");
            await operation;
        }

        private static async Task<T> CompleteWithin<T>(Task<T> operation)
        {
            Task winner = await Task.WhenAny(operation, Task.Delay(OperationTimeout));
            if (!ReferenceEquals(winner, operation)) throw new TimeoutException("Firebase request timed out");
            return await operation;
        }

        private static bool IsSafeHttps(string value) =>
            Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme == Uri.UriSchemeHttps;

        private static string NormalizeDisplayMode(string value, string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return "template";
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            return normalized == "full_banner" || normalized == "background" ? normalized : "template";
        }
    }
}
