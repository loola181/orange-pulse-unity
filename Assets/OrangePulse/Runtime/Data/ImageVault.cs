using System;
using System.IO;
using UnityEngine;

namespace OrangePulse.Data
{
    public sealed class ImageVault
    {
        private readonly string _avatarPath = Path.Combine(Application.persistentDataPath, "orange-avatar.jpg");

        public bool TryImport(string sourcePath, out Texture2D texture, out string savedPath, out string error)
        {
            texture = null;
            savedPath = null;
            error = null;

            try
            {
                if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                    throw new FileNotFoundException("Выбранное изображение не найдено");

                byte[] bytes = File.ReadAllBytes(sourcePath);
                var decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!decoded.LoadImage(bytes, false))
                {
                    UnityEngine.Object.Destroy(decoded);
                    throw new InvalidDataException("Формат изображения не поддерживается");
                }

                texture = LimitSize(decoded, 1024);
                byte[] jpeg = texture.EncodeToJPG(88);
                File.WriteAllBytes(_avatarPath, jpeg);
                savedPath = _avatarPath;
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                if (texture != null) UnityEngine.Object.Destroy(texture);
                texture = null;
                return false;
            }
        }

        public bool TryLoad(string path, out Texture2D texture)
        {
            texture = null;
            try
            {
                string candidate = string.IsNullOrWhiteSpace(path) ? _avatarPath : path;
                if (!File.Exists(candidate)) return false;
                var decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!decoded.LoadImage(File.ReadAllBytes(candidate), false))
                {
                    UnityEngine.Object.Destroy(decoded);
                    return false;
                }
                texture = decoded;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[OrangePulse] Avatar read failed: {exception.Message}");
                return false;
            }
        }

        private static Texture2D LimitSize(Texture2D source, int maxSide)
        {
            if (source.width <= maxSide && source.height <= maxSide) return source;

            float scale = maxSide / (float)Mathf.Max(source.width, source.height);
            int width = Mathf.Max(1, Mathf.RoundToInt(source.width * scale));
            int height = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));
            RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(source, temporary);
            RenderTexture.active = temporary;

            var resized = new Texture2D(width, height, TextureFormat.RGBA32, false);
            resized.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            resized.Apply(false, false);

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
            UnityEngine.Object.Destroy(source);
            return resized;
        }
    }
}

