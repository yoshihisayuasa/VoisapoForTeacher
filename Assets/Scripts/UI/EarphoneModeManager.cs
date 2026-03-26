using UnityEngine;

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

        /// <summary>
        /// 現在のイヤホンモード状態
        /// </summary>
        public bool EarphoneMode => _earphoneMode;

        // UI 層などから呼び出す設定 API（Infra -> UI の依存を排除）
        public void SetMode(bool on)
        {
            _earphoneMode = on;
        }
    }
}
