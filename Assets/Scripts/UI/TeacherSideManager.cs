using Assets.Scripts.UI.Piano;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(Toggle))]
    [RequireComponent(typeof(Selectable))]
    public class TeacherSideManager : MonoBehaviour
    {
        public static TeacherSideManager Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _image;

        private readonly Color _onColor = AppColors.Accent;
        private readonly Color _offColor = Color.white;

        private bool _teacherSideButtonState;

        public bool TeacherSideButtonState
        {
            get => _teacherSideButtonState;
            private set
            {
                if (!value && IsCtrlHeld())
                {
                    return;
                }
                if (_teacherSideButtonState == value)
                {
                    return;
                }

                _teacherSideButtonState = value;
                SyncToggle(value);
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
                .AddTo(PianoController.Instance);
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
            if (IsCtrlPressedThisFrame())
            {
                TeacherSideButtonState = true;
            }
            else if (IsCtrlReleasedThisFrame())
            {
                TeacherSideButtonState = false;
            }
        }

        private static bool IsCtrlHeld()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                return false;
            }
            if (kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed)
            {
                return true;
            }
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            if (kb.leftCommandKey.isPressed || kb.rightCommandKey.isPressed)
            {
                return true;
            }
#endif
            return false;
        }

        private static bool IsCtrlPressedThisFrame()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                return false;
            }
            if (kb.leftCtrlKey.wasPressedThisFrame || kb.rightCtrlKey.wasPressedThisFrame)
            {
                return true;
            }
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            if (kb.leftCommandKey.wasPressedThisFrame || kb.rightCommandKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
            return false;
        }

        private static bool IsCtrlReleasedThisFrame()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                return false;
            }
            if (kb.leftCtrlKey.wasReleasedThisFrame || kb.rightCtrlKey.wasReleasedThisFrame)
            {
                return true;
            }
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            if (kb.leftCommandKey.wasReleasedThisFrame || kb.rightCommandKey.wasReleasedThisFrame)
            {
                return true;
            }
#endif
            return false;
        }
    }
}
