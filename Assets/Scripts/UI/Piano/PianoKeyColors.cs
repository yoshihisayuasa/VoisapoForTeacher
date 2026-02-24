using UnityEngine;

namespace Scripts.UI.Piano
{
    public static class PianoKeyColors
    {
        private static readonly Color WhitePlayed = new(1f, 0.6f, 0.8f);
        private static readonly Color BlackPlayed = new(0.8f, 0.2f, 0.5f);
        private static readonly Color WhiteMin = Color.yellow;
        private static readonly Color WhiteMax = Color.blue;
        private static readonly Color BlackMin = new(0.5f, 0.5f, 0.0f);
        private static readonly Color BlackMax = new(0f, 0f, 0.5f);

        public static readonly Color Playing = Color.red;
        public static Color SetPlayedColor(bool sharp) => sharp ? BlackPlayed : WhitePlayed;
        public static Color SetMinColor(bool sharp) => sharp ? BlackMin : WhiteMin;
        public static Color SetMaxColor(bool sharp) => sharp ? BlackMax : WhiteMax;
    }
}