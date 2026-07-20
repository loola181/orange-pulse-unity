using UnityEngine;

namespace OrangePulse.Presentation.Pages
{
    public abstract class PageSurface
    {
        public RectTransform Root { get; protected set; }

        public void SetVisible(bool visible)
        {
            if (Root != null) Root.gameObject.SetActive(visible);
        }
    }
}

