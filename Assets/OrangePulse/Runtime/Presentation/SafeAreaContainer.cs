using UnityEngine;

namespace OrangePulse.Presentation
{
    public sealed class SafeAreaContainer : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastArea;
        private Vector2Int _lastScreen;

        private void Awake()
        {
            _rect = transform as RectTransform;
            Apply();
        }

        private void Update()
        {
            if (_lastArea != Screen.safeArea || _lastScreen.x != Screen.width || _lastScreen.y != Screen.height)
                Apply();
        }

        private void Apply()
        {
            if (_rect == null || Screen.width <= 0 || Screen.height <= 0) return;

            Rect area = Screen.safeArea;
            _rect.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            _rect.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
            _lastArea = area;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
        }
    }
}

