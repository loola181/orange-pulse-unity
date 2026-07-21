using System;
using System.IO;
using OrangePulse.Core;
using UnityEngine;

namespace OrangePulse.Data
{
    public sealed class ProfileStore
    {
        private readonly string _path = Path.Combine(Application.persistentDataPath, "orange-profile.json");

        public ProfileData Load()
        {
            try
            {
                if (!File.Exists(_path)) return new ProfileData();
                ProfileData profile = JsonUtility.FromJson<ProfileData>(File.ReadAllText(_path));
                return Normalize(profile ?? new ProfileData());
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangePulse] Profile read failed: {exception.Message}");
                return new ProfileData();
            }
        }

        public void Save(ProfileData profile)
        {
            if (profile == null) return;
            Normalize(profile);

            try
            {
                File.WriteAllText(_path, JsonUtility.ToJson(profile, true));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangePulse] Profile save failed: {exception.Message}");
            }
        }

        public static string NormalizeName(string value)
        {
            string normalized = string.IsNullOrWhiteSpace(value) ? "Игрок 12" : value.Trim();
            return normalized.Length > 24 ? normalized.Substring(0, 24) : normalized;
        }

        public static string NormalizeLeagueId(string value)
        {
            foreach (LeagueSource league in AppEndpoints.FeaturedLeagues)
            {
                if (league.Id == value) return value;
            }
            return AppEndpoints.FeaturedLeagues[0].Id;
        }

        public static int ActivityPoints(ProfileData profile)
        {
            if (profile == null) return 0;
            return Math.Max(0, profile.openedStories) * 10 +
                   Math.Max(0, profile.refreshedFeeds) * 15 +
                   Math.Max(0, profile.openedMatchCenters) * 25 +
                   Math.Max(0, profile.openedScorers) * 10;
        }

        public static int Level(ProfileData profile) => 1 + ActivityPoints(profile) / 100;

        public static int LevelProgress(ProfileData profile) => ActivityPoints(profile) % 100;

        public static string LevelTitle(ProfileData profile) => Level(profile) switch
        {
            1 => "НОВИЧОК",
            2 => "БОЛЕЛЬЩИК",
            3 => "ЗНАТОК",
            4 => "АНАЛИТИК",
            _ => "ЭКСПЕРТ"
        };

        private static ProfileData Normalize(ProfileData profile)
        {
            profile.nickname = NormalizeName(profile.nickname);
            profile.favoriteLeagueId = NormalizeLeagueId(profile.favoriteLeagueId);
            profile.openedStories = Math.Max(0, profile.openedStories);
            profile.refreshedFeeds = Math.Max(0, profile.refreshedFeeds);
            profile.openedMatchCenters = Math.Max(0, profile.openedMatchCenters);
            profile.openedScorers = Math.Max(0, profile.openedScorers);
            return profile;
        }
    }
}
