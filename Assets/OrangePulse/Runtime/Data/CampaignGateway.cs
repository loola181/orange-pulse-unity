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
            string subtitle)
        {
            string action = (clickAction ?? string.Empty).Trim().ToLowerInvariant();
            string safeActionUrl = action == "url" && IsSafeHttps(clickUrl) ? clickUrl : LeagueUrl;
            string safeImageUrl = IsSafeHttps(imageUrl) ? imageUrl : string.Empty;

            return new Campaign
            {
                Enabled = enabled && launchEnabled,
                Eyebrow = "FIREBASE · LIVE",
                Title = string.IsNullOrWhiteSpace(title) ? "Главный матч недели" : title.Trim(),
                Body = string.IsNullOrWhiteSpace(subtitle)
                    ? "Расписание и новости футбола в одном приложении."
                    : subtitle.Trim(),
                ButtonLabel = action == "url" ? "ОТКРЫТЬ" : "СМОТРЕТЬ МАТЧИ",
                ButtonUrl = safeActionUrl,
                ImageUrl = safeImageUrl
            };
        }

        private static async Task<Campaign> FetchCampaign()
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
                config.GetValue(EnabledKey).BooleanValue,
                config.GetValue(LaunchEnabledKey).BooleanValue,
                config.GetValue(ClickActionKey).StringValue,
                config.GetValue(ClickUrlKey).StringValue,
                config.GetValue(ImageUrlKey).StringValue,
                config.GetValue(TitleKey).StringValue,
                config.GetValue(SubtitleKey).StringValue);
        }

        private static Dictionary<string, object> DefaultValues() => new()
        {
            { EnabledKey, true },
            { LaunchEnabledKey, true },
            { ClickActionKey, "url" },
            { ClickUrlKey, LeagueUrl },
            { ImageUrlKey, string.Empty },
            { ImageRevisionKey, "builtin-v1" },
            { TitleKey, "Главный матч недели" },
            { SubtitleKey, "Расписание и новости футбола в одном приложении." },
            { DisplayModeKey, "background" }
        };

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
    }
}
