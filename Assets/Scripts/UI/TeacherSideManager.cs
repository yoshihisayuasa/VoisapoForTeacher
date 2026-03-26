using System;
using Assets.Scripts.UI.Piano;
using UnityEngine;
using UnityEngine.InputSystem;
using R3;

namespace Assets.Scripts.UI
{
    public class TeacherSideManager : MonoBehaviour
    {
        public static TeacherSideManager Instance { get; private set; }
        private bool _teacherSideButtonState;
        public event Action<bool> TeacherSideButtonStateChanged;

        public bool TeacherSideButtonState
        {
            get => _teacherSideButtonState;
            set
            {
                if (!value && IsCtrlHeld())
                { 
                    return;
                }
                if (_teacherSideButtonState != value)
                {
                    _teacherSideButtonState = value;
                    TeacherSideButtonStateChanged?.Invoke(_teacherSideButtonState);
                }
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
            if(kb.leftCommandKey.wasPressedThisFrame || kb.rightCommandKey.wasPressedThisFrame)
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
            if(kb.leftCtrlKey.wasReleasedThisFrame || kb.rightCtrlKey.wasReleasedThisFrame)
            {
                return true;
            }
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            if(kb.leftCommandKey.wasReleasedThisFrame || kb.rightCommandKey.wasReleasedThisFrame)
            {
                return true;
            }
#endif
            return false;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }

        private void Start()
        {
            PianoController.Instance.OnAnyKeyUpAsObservable
                .Subscribe(_ => TeacherSideButtonState = false)
                .AddTo(PianoController.Instance);
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
    }
}
