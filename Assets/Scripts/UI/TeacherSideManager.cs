using System;
using UnityEngine;

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
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                || Input.GetKeyDown(KeyCode.LeftCommand) || Input.GetKeyDown(KeyCode.RightCommand)
#endif
                           )
            {
                TeacherSideButtonState = true;
            }

            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                || Input.GetKeyUp(KeyCode.LeftCommand) || Input.GetKeyUp(KeyCode.RightCommand)
#endif
                    )
            {
                TeacherSideButtonState = false;

            }

        }
    }
}
