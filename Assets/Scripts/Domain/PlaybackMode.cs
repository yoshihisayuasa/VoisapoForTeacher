namespace Scripts.Domain
{
    public readonly struct PlayModeSettings
    {
        public bool PlayCode { get; }
        public bool PlayPiano { get; }
        public bool PlayMetronome { get; }
        private PlayModeSettings(bool playCode, bool playMetronome, bool playPiano)
        {
            PlayCode = playCode;
            PlayPiano = playPiano;
            PlayMetronome = playMetronome;
        }

        public static PlayModeSettings FromFlags(bool isTeacherSide, bool earphoneOn)
        {
            if (isTeacherSide)
            {
                // êÊê∂ë§: ÉsÉAÉmâπÅ{ÉÅÉgÉçÉmÅ[ÉÄ
                return new PlayModeSettings(true, true, true);
            }
            else if (earphoneOn)
            {
                return new PlayModeSettings(true, true, false);
            }
            else
            {
                // ê∂ìkë§
                return new PlayModeSettings(false, false, false);
            }
        }
    }
}