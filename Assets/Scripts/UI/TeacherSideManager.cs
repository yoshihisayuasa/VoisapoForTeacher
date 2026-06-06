using Assets.Scripts.UI.Piano;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

        public bool TeacherSideButtonState
        {
            get => _teacherSideButtonState;
            private set
            {
                if (!value && IsShiftHeld())
                {
                    return;
                }
                if (_teacherSideButtonState == value)
                {
                    return;
                }

                _teacherSideButtonState = value;
                SyncToggle(value);
                _onStateChanged.OnNext(value);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_toggle == null)
            {
                _toggle = GetComponent<Toggle>();
            }
            if (_image == null)
            {
                if (GetComponent<Selectable>().targetGraphic is Image img)
                {
                    _image = img;
                }
            }
        }



        private void Start()
        {
            _toggle.onValueChanged.AddListener(isOn => TeacherSideButtonState = isOn);

            PianoController.Instance.OnAnyKeyUpAsObservable
                .Subscribe(_ => TeacherSideButtonState = false)
                .AddTo(this);
        }

        private void SyncToggle(bool isOn)
        {
            if (_toggle != null && _toggle.isOn != isOn)
            {
                _toggle.isOn = isOn;
            }
            if (_image != null)
            {
                _image.color = isOn ? _onColor : _offColor;
            }
        }

        private void Update()
        {
            if (IsShiftPressedThisFrame())
            {
                TeacherSideButtonState = true;
            }
            else if (IsShiftReleasedThisFrame())
            {
                TeacherSideButtonState = false;
            }
        }

        private static bool IsShiftHeld()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                return false;
            }
            return kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
        }

        private static bool IsShiftPressedThisFrame()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                return false;
            }
            return kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame;
        }

        private static bool IsShiftReleasedThisFrame()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                return false;
            }
            return kb.leftShiftKey.wasReleasedThisFrame || kb.rightShiftKey.wasReleasedThisFrame;
        }
    }
}
