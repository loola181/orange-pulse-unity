using System;
using System.IO;
using UnityEngine;

namespace OrangePulse.Data
{
    public sealed class DiskTextCache
    {
        private readonly string _directory;

        public DiskTextCache()
        {
            _directory = Path.Combine(Application.persistentDataPath, "feed-cache");
        }

        public void Put(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            try
            {
                Directory.CreateDirectory(_directory);
                string target = PathFor(key);
                string temporary = target + ".new";
                File.WriteAllText(temporary, value);
                if (File.Exists(target)) File.Delete(target);
                File.Move(temporary, target);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangePulse] Cache write failed: {exception.Message}");
            }
        }

        public bool TryGet(string key, TimeSpan maxAge, out string value)
        {
            value = null;
            try
            {
                string path = PathFor(key);
                if (!File.Exists(path)) return false;
                if (DateTime.UtcNow - File.GetLastWriteTimeUtc(path) > maxAge) return false;
                value = File.ReadAllText(path);
                return !string.IsNullOrWhiteSpace(value);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangePulse] Cache read failed: {exception.Message}");
                return false;
            }
        }

        private string PathFor(string key)
        {
            string safeKey = string.IsNullOrWhiteSpace(key)
                ? "empty"
                : key.Replace("/", "-").Replace("\\", "-").Replace("..", "-");
            return Path.Combine(_directory, safeKey + ".txt");
        }
    }
}

