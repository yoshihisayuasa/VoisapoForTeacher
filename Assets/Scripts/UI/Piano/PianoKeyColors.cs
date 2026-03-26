using UnityEngine;

namespace AsseScripts.UI.Piano
{
    public static class PianoKeyColors
    {
        private static readonly Color WhitePlayed = new(1.000f, 0.678f, 0.831f); // #FFADD4
        private static readonly Color BlackPlayed = new(0.698f, 0.314f, 0.537f); // #B25089
        private static readonly Color WhiteMin = new(0.659f, 0.816f, 0.553f); // #A8D08D
        private static readonly Color WhiteMax = new(0.357f, 0.784f, 0.910f); // #5BC8E8
        private static readonly Color BlackMin = new(0.290f, 0.478f, 0.227f); // #4A7A3A
        private static readonly Color BlackMax = new(0.157f, 0.502f, 0.627f); // #285080

        public static readonly Color Playing = new(1.000f, 0.302f, 0.651f); // #FF4DA6
        public static Color SetPlayedColor(bool sharp) => sharp ? BlackPlayed : WhitePlayed;
        public static Color SetMinColor(bool sharp) => sharp ? BlackMin : WhiteMin;
        public static Color SetMaxColor(bool sharp) => sharp ? BlackMax : WhiteMax;
    }
}