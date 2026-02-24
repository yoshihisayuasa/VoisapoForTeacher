using System;

namespace Assets.Scripts.UI
{
    public class AutoKeyChangeManager
    {
        public static AutoKeyChangeManager Instance { get; } = new AutoKeyChangeManager();
        private AutoKeyChangeState _state = AutoKeyChangeState.None;
        private AutoKeyChangeState _lastDirection = AutoKeyChangeState.None;
        private bool _repeatOncePending;
        public AutoKeyChangeState State => _state;
        public event Action<AutoKeyChangeState> OnStateChanged;
        public bool RepeatOncePending => _repeatOncePending;

        public void SetState(AutoKeyChangeState newState)
        {
            if (_state == newState)
            {
                return;
            }
            _state = newState;
            OnStateChanged?.Invoke(_state);
        }

        public void Toggle(AutoKeyChangeState target)
        {
            if (_state == target)
            {
                SetState(AutoKeyChangeState.None);
            }
            else
            {
                SetState(target);
            }
        }
        public void TriggerRepeatOnce()
        {
            if (_lastDirection == AutoKeyChangeState.None)
            {
                return;
            }
            _repeatOncePending = true;
            OnStateChanged?.Invoke(_state);
        }
        public void ClearRepeatOnce()
        {
            if (_repeatOncePending)
            {
                _repeatOncePending = false;
                OnStateChanged?.Invoke(_state);
            }
        }

        public enum AutoKeyChangeState
        {
            None,
            Up,
            Down,
        }
    }

}
