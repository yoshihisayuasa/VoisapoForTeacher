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

        public void SetState(bool value)
        {
            if (_isSoundPlay == value)
            {
                return;
            }

            _isSoundPlay = value;
            _onStateChanged.OnNext(value);
        }
    }
}
