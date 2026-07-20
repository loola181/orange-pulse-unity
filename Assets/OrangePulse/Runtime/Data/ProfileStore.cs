using System;
using System.IO;
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
                return profile ?? new ProfileData();
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
            profile.nickname = NormalizeName(profile.nickname);

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
    }
}

