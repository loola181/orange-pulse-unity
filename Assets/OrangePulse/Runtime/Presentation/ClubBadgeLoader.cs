using System;
using System.Collections;
using System.Collections.Generic;
using OrangePulse.Core;
using OrangePulse.Data;
using UnityEngine;
using UnityEngine.UI;

namespace OrangePulse.Presentation
{
    public sealed class ClubBadgeLoader : MonoBehaviour
    {
        private const int MaxConcurrentDownloads = 6;
        private readonly Dictionary<string, Sprite> _cache = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<Image>> _waiting = new(StringComparer.Ordinal);
        private readonly Queue<string> _queue = new();
        private HttpTransport _transport;
        private int _activeDownloads;

        public void Initialize(HttpTransport transport)
        {
            _transport = transport;
        }

        public void Load(Image target, string url)
        {
            if (target == null) return;
            target.sprite = null;
            target.color = Color.clear;
            target.preserveAspect = true;
            target.raycastTarget = false;
            if (_transport == null || !IsSupportedUrl(url)) return;

            if (_cache.TryGetValue(url, out Sprite cached))
            {
                Apply(target, cached);
                return;
            }

            if (_waiting.TryGetValue(url, out List<Image> targets))
            {
                targets.Add(target);
                return;
            }

            _waiting[url] = new List<Image> { target };
            _queue.Enqueue(url);
            PumpQueue();
        }

        public static bool IsSupportedUrl(string value) =>
            Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme == Uri.UriSchemeHttps;

        private void PumpQueue()
        {
            while (_activeDownloads < MaxConcurrentDownloads && _queue.Count > 0)
            {
                string url = _queue.Dequeue();
                _activeDownloads++;
                StartCoroutine(Download(url));
            }
        }

        private IEnumerator Download(string url)
        {
            LoadResult<Texture2D> result = null;
            yield return _transport.GetTexture(url, value => result = value);

            Sprite sprite = null;
            if (result != null && result.IsSuccess && result.Data != null)
            {
                sprite = VisualComposer.FromTexture(result.Data, "ClubBadge");
                _cache[url] = sprite;
            }

            if (_waiting.Remove(url, out List<Image> targets) && sprite != null)
            {
                foreach (Image target in targets)
                {
                    if (target != null) Apply(target, sprite);
                }
            }

            _activeDownloads--;
            PumpQueue();
        }

        private static void Apply(Image target, Sprite sprite)
        {
            target.sprite = sprite;
            target.color = Color.white;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            foreach (Sprite sprite in _cache.Values)
            {
                if (sprite == null) continue;
                Texture2D texture = sprite.texture;
                Destroy(sprite);
                if (texture != null) Destroy(texture);
            }
            _cache.Clear();
            _waiting.Clear();
            _queue.Clear();
        }
    }
}
