using Assets.Scripts.UI.Piano;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(Toggle))]
    [RequireComponent(typeof(Selectable))]
    public sealed class TeacherSideManager : MonoBehaviour
    {
        public static TeacherSideManager Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _image;

        private readonly Color _onColor = AppColors.Accent;
        private readonly Color _offColor = Color.white;

        private bool _teacherSideButtonState;

        private readonly Subject<bool> _onStateChanged = new();
        public Observable<bool> OnStateChanged => _onStateChanged;

        public bool TeacherSideButtonState => _teacherSideButtonState;

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
            if (_teacherSideButtonState == value) return;

            _teacherSideButtonState = value;
            SyncToggle(value);
            _onStateChanged.OnNext(value);
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
