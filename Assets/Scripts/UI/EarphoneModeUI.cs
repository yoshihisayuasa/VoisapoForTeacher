using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// イヤホンモードボタンUI制御
    /// 他のクラスにボタンのオンオフ状態を通知する
    /// 生徒が入室していない間はグレーアウトして操作不可にする
    /// </summary>
    public sealed class EarphoneModeUI : MonoBehaviour
    {
        [SerializeField] private Button _earphoneModeButton;
        [SerializeField] private Image _targetImage;                // 色を変える対象（必ず割り当てる）
        [SerializeField] private ConnectionStatusObserver _observer;

        private bool _isEarphoneModeOn = false;
        private bool _isParticipantPresent = false;

        void Start()
        {
            if (EarphoneModeManager.Instance != null)
            {
                _isEarphoneModeOn = EarphoneModeManager.Instance.EarphoneMode;
            }
            _earphoneModeButton.onClick.AddListener(OnButtonClicked);

            _observer.OnParticipantJoined.Subscribe(_ => SetParticipantPresent(true)).AddTo(this);
            _observer.OnParticipantLeft.Subscribe(_ => SetParticipantPresent(false)).AddTo(this);
            _observer.OnSelfDisconnected.Subscribe(_ => SetParticipantPresent(false)).AddTo(this);

            SetParticipantPresent(false);
        }

        // ボタンがクリックされたときにオンオフを切り替えて通知
        public void OnButtonClicked()
        {
            _isEarphoneModeOn = !_isEarphoneModeOn;
            // Infra へ伝達（UI -> Infra のみ）
            if (EarphoneModeManager.Instance != null)
            {
                EarphoneModeManager.Instance.SetMode(_isEarphoneModeOn);
            }
            UpdateVisual();
        }

        private void SetParticipantPresent(bool isPresent)
        {
            _isParticipantPresent = isPresent;
            _earphoneModeButton.interactable = isPresent;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            _targetImage.color = _isParticipantPresent
                ? AppColors.ActiveOrWhite(_isEarphoneModeOn)
                : AppColors.Disabled;
        }
    }
}
