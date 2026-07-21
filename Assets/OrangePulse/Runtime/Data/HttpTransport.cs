using System;
using System.Collections;
using OrangePulse.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace OrangePulse.Data
{
    public sealed class HttpTransport
    {
        public IEnumerator GetText(string url, Action<LoadResult<string>> finished)
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 18;
            request.SetRequestHeader("Accept", "application/json, application/xml, text/xml, */*");
            request.SetRequestHeader("User-Agent", "OrangeFootball/1.0 (Unity Android)");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success &&
                !string.IsNullOrWhiteSpace(request.downloadHandler.text))
            {
                finished?.Invoke(LoadResult<string>.Fresh(request.downloadHandler.text));
                yield break;
            }

            string detail = string.IsNullOrWhiteSpace(request.error)
                ? $"HTTP {request.responseCode}"
                : request.error;
            finished?.Invoke(LoadResult<string>.Failed(detail));
        }

        public IEnumerator GetTexture(string url, Action<LoadResult<Texture2D>> finished)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url, true);
            request.timeout = 18;
            request.SetRequestHeader("User-Agent", "OrangeFootball/1.0 (Unity Android)");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                finished?.Invoke(LoadResult<Texture2D>.Fresh(texture));
                yield break;
            }

            finished?.Invoke(LoadResult<Texture2D>.Failed(request.error ?? "Image request failed"));
        }
    }
}
