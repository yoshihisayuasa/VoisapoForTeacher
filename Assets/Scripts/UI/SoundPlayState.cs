using R3;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 「音を再生する側か」の状態を保持・通知する純粋な状態オブジェクト。
    /// UI も Unity ライフサイクルも持たないため、先生・生徒どちらのビルドからも参照できる。
    /// 先生ビルドは PlaySideManager（UI/入力）が、生徒ビルドは PianoNetworkGateway（受信）が駆動する。
    /// </summary>
    public sealed class SoundPlayState
    {
        public static SoundPlayState Instance { get; } = new();

        private bool _isSoundPlay = false;
        private readonly Subject<bool> _onStateChanged = new();

        public bool IsSoundPlay => _isSoundPlay;
        public Observable<bool> OnStateChanged => _onStateChanged;

        // ── 先生専用：再生側の「意図」（トグル選択）と接続相手の有無から実効状態を決める ──
        private bool _teacherIntent = false;
        private bool _studentConnected = false;
        private readonly Subject<bool> _onIntentChanged = new();

        public bool TeacherIntent => _teacherIntent;
        public Observable<bool> OnIntentChanged => _onIntentChanged;

        /// <summary>
        /// 受信した再生状態をそのまま実効状態へ反映する（生徒ビルドの受信、および内部の再計算から呼ぶ）。
        /// </summary>
        public void SetState(bool value)
        {
            if (_isSoundPlay == value)
            {
                return;
            }

            _isSoundPlay = value;
            _onStateChanged.OnNext(value);
        }

        /// <summary>
        /// 先生が選んだ再生側（トグル）の意図を設定する。実際に鳴る側は接続相手の有無を加味して決まる。
        /// </summary>
        public void SetTeacherIntent(bool value)
        {
            if (_teacherIntent != value)
            {
                _teacherIntent = value;
                _onIntentChanged.OnNext(value);
            }

            RecomputeForTeacher();
        }

        /// <summary>
        /// 生徒が接続しているかを反映する。生徒がいなければ先生自身が鳴らす。
        /// </summary>
        public void SetStudentConnected(bool connected)
        {
            _studentConnected = connected;
            RecomputeForTeacher();
        }

        /// <summary>
        /// 生徒がいれば先生の意図に従い、いなければ先生が鳴らす。
        /// </summary>
        private void RecomputeForTeacher()
        {
            SetState(!_studentConnected || _teacherIntent);
        }
    }
}
