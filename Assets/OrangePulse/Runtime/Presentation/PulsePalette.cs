using UnityEngine;

namespace OrangePulse.Presentation
{
    public static class PulsePalette
    {
        public static readonly Color Orange = Hex("FF6B00");
        public static readonly Color OrangeSoft = Hex("FF9A45");
        public static readonly Color Ink = Hex("101010");
        public static readonly Color InkSoft = Hex("252525");
        public static readonly Color Paper = Hex("FFF8EE");
        public static readonly Color White = Hex("FFFFFF");
        public static readonly Color Muted = Hex("756F68");
        public static readonly Color Line = Hex("E7DED3");
        public static readonly Color Success = Hex("228B55");
        public static readonly Color Danger = Hex("C9402C");

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color color);
            return color;
        }
    }
}

