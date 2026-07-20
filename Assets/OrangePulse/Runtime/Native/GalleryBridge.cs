using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OrangePulse.Native
{
    public sealed class GalleryBridge : MonoBehaviour
    {
        public event Action<string> ImageSelected;
        public event Action<string> SelectionFailed;

        public void Open()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var picker = new AndroidJavaClass("com.orangepulse.mobile.OrangeGalleryPicker");
                picker.CallStatic("pick", gameObject.name);
            }
            catch (Exception exception)
            {
                SelectionFailed?.Invoke(exception.Message);
            }
#elif UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel("Выберите изображение", string.Empty, "png,jpg,jpeg");
            if (!string.IsNullOrWhiteSpace(path)) ImageSelected?.Invoke(path);
#else
            SelectionFailed?.Invoke("Галерея поддерживается на Android");
#endif
        }

        public void OnGalleryPicked(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                SelectionFailed?.Invoke("Изображение не выбрано");
                return;
            }
            ImageSelected?.Invoke(path);
        }

        public void OnGalleryError(string message)
        {
            if (string.Equals(message, "cancelled", StringComparison.OrdinalIgnoreCase)) return;
            SelectionFailed?.Invoke(string.IsNullOrWhiteSpace(message)
                ? "Не удалось открыть изображение"
                : message);
        }
    }
}

