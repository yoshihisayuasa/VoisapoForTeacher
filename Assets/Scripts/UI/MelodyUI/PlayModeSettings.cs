namespace Assets.Scripts.UI.MelodyUI
{
    /// <summary>
    /// 再生時に何を鳴らすか（和音・ピアノ・メトロノーム）の設定。
    /// </summary>
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
                // 先生側: コード＋メトロノーム＋ピアノ
                return new PlayModeSettings(true, true, true);
            }
            else if (earphoneOn)
            {
                // イヤホンモード: コード＋メトロノーム（ピアノなし）
                return new PlayModeSettings(true, true, false);
            }
            else
            {
                // 生徒側: すべてオフ
                return new PlayModeSettings(false, false, false);
            }
        }
    }
}
