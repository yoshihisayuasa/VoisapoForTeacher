using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
                if (_teacherSideButtonState != value)
                {
                    _teacherSideButtonState = value;
                    TeacherSideButtonStateChanged?.Invoke(_teacherSideButtonState);
                }
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
            DontDestroyOnLoad(gameObject);

        }
        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // 押された瞬間（GetKeyDown 相当）
            bool down =
                kb.leftCtrlKey.wasPressedThisFrame ||
                kb.rightCtrlKey.wasPressedThisFrame ||
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                kb.leftCommandKey.wasPressedThisFrame ||
                kb.rightCommandKey.wasPressedThisFrame ||
#endif
                false;

            if (down)
            {
                TeacherSideButtonState = true;
            }

            // 離された瞬間（GetKeyUp 相当）
            bool up =
                kb.leftCtrlKey.wasReleasedThisFrame ||
                kb.rightCtrlKey.wasReleasedThisFrame ||
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                kb.leftCommandKey.wasReleasedThisFrame ||
                kb.rightCommandKey.wasReleasedThisFrame ||
#endif
                false;

            if (up)
            {
                TeacherSideButtonState = false;
            }

        }
    }
}
