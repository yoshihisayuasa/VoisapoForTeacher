using Assets.Scripts.UI.Piano;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(Toggle))]
    [RequireComponent(typeof(Selectable))]
    public sealed class PlaySideManager : MonoBehaviour
    {
        public static PlaySideManager Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _image;

        private readonly Color _onColor = AppColors.Accent;
        private readonly Color _offColor = Color.white;

        private bool _isSoundPlay = false;

        private readonly Subject<bool> _onStateChanged = new();
        public Observable<bool> OnStateChanged => _onStateChanged;

        public bool IsSoundPlay => _isSoundPlay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_toggle == null) _toggle = GetComponent<Toggle>();
            if (_image == null && GetComponent<Selectable>().targetGraphic is Image img) _image = img;
        }

        private void Start()
        {
            _toggle.onValueChanged.AddListener(SetState);

            PianoController.Instance.OnAnyKeyUpAsObservable
                .Subscribe(_ => SetState(false))
                .AddTo(this);
        }

        public void SetState(bool value)
        {
            if (!value && IsShiftHeld()) return;
            if (_isSoundPlay == value) return;

            _isSoundPlay = value;
            SyncToggle(value);
            _onStateChanged.OnNext(value);
        }

        /// <summary>
        /// 先生から受信した状態を反映する（生徒ビルド用）。
        /// 送信側で反転済みの値がそのまま渡る。
        /// </summary>
        public void ApplyRemoteSoundPlayState(bool isSoundPlay)
        {
            if (_isSoundPlay == isSoundPlay) return;

            _isSoundPlay = isSoundPlay;
            SyncToggle(isSoundPlay);
            _onStateChanged.OnNext(isSoundPlay);
        }

        private void SyncToggle(bool isOn)
        {
            if (_toggle != null && _toggle.isOn != isOn) _toggle.isOn = isOn;
            if (_image != null) _image.color = isOn ? _onColor : _offColor;
        }

        private void Update()
        {
            if (IsShiftPressedThisFrame())       SetState(true);
            else if (IsShiftReleasedThisFrame()) SetState(false);
        }

        private static bool IsShiftHeld()
        {
            var kb = Keyboard.current;
            return kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
        }

        private static bool IsShiftPressedThisFrame()
        {
            var kb = Keyboard.current;
            return kb != null && (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame);
        }

        private static bool IsShiftReleasedThisFrame()
        {
            var kb = Keyboard.current;
            return kb != null && (kb.leftShiftKey.wasReleasedThisFrame || kb.rightShiftKey.wasReleasedThisFrame);
        }
    }
}
