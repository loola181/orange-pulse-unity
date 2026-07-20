using System;
using System.Collections;
using OrangePulse.Core;
using UnityEngine;

namespace OrangePulse.Data
{
    public sealed class CampaignGateway
    {
        private readonly HttpTransport _transport;
        private readonly DiskTextCache _cache;

        public CampaignGateway(HttpTransport transport, DiskTextCache cache)
        {
            _transport = transport;
            _cache = cache;
        }

        public IEnumerator Load(Action<LoadResult<Campaign>> finished)
        {
            LoadResult<string> response = null;
            yield return _transport.GetText(AppEndpoints.CampaignConfig, value => response = value);

            string payload;
            bool fromCache = false;
            if (response != null && response.IsSuccess)
            {
                payload = response.Data;
                _cache.Put("campaign", payload);
            }
            else if (_cache.TryGet("campaign", TimeSpan.FromDays(14), out string cached))
            {
                payload = cached;
                fromCache = true;
            }
            else
            {
                finished?.Invoke(LoadResult<Campaign>.Failed(response?.Error ?? "Баннер недоступен"));
                yield break;
            }

            try
            {
                Campaign campaign = Parse(payload);
                finished?.Invoke(fromCache
                    ? LoadResult<Campaign>.Cached(campaign)
                    : LoadResult<Campaign>.Fresh(campaign));
            }
            catch (Exception exception)
            {
                finished?.Invoke(LoadResult<Campaign>.Failed(exception.Message));
            }
        }

        public static Campaign Parse(string json)
        {
            CampaignDto source = JsonUtility.FromJson<CampaignDto>(json);
            if (source == null) throw new FormatException("Пустая конфигурация баннера");
            if (!IsSafeHttps(source.button_url)) throw new FormatException("Небезопасная ссылка баннера");
            if (!string.IsNullOrWhiteSpace(source.image_url) && !IsSafeHttps(source.image_url))
                throw new FormatException("Небезопасное изображение баннера");

            return new Campaign
            {
                Enabled = source.enabled,
                Eyebrow = source.eyebrow ?? string.Empty,
                Title = source.title ?? string.Empty,
                Body = source.body ?? string.Empty,
                ButtonLabel = string.IsNullOrWhiteSpace(source.button_label) ? "ОТКРЫТЬ" : source.button_label,
                ButtonUrl = source.button_url,
                ImageUrl = source.image_url ?? string.Empty
            };
        }

        private static bool IsSafeHttps(string value) =>
            Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme == Uri.UriSchemeHttps;
    }
}

