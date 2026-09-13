using R3;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// イヤホンモード管理クラス（シングルトン）
    /// 他クラスにイヤホンモードの状態を提供する
    /// </summary>
    public class EarphoneModeManager
    {
        public static EarphoneModeManager Instance { get; } = new EarphoneModeManager();
        private bool _earphoneMode = false;
        private readonly ReactiveProperty<bool> _studentConnected = new(false);

        private readonly Subject<bool> _onModeChanged = new();
        public Observable<bool> OnModeChanged => _onModeChanged;

        /// <summary>
        /// 現在のイヤホンモード状態
        /// </summary>
        public bool EarphoneMode => _earphoneMode;

        /// <summary>
        /// イヤホンモードを切り替えられるか。イヤホンモードは生徒側で鳴らすときの設定なので、
        /// 生徒がいない（先生が鳴らすしかない）間は切り替えても意味がない。
        /// </summary>
        public ReadOnlyReactiveProperty<bool> CanToggle => _studentConnected;

        public void Toggle()
        {
            SetMode(!_earphoneMode);
        }

        // UI 層などから呼び出す設定 API（Infra -> UI の依存を排除）
        public void SetMode(bool on)
        {
            var value = on && _studentConnected.Value;
            if (_earphoneMode == value)
            {
                return;
            }
            _earphoneMode = value;
            _onModeChanged.OnNext(value);
        }

        /// <summary>
        /// 生徒が接続しているかを反映する。
        /// 退室してもモードは戻さない。イヤホンを着けているかは先生側の環境なので、再入室後もそのまま使える。
        /// </summary>
        public void SetStudentConnected(bool connected)
        {
            _studentConnected.Value = connected;
        }
    }
}
