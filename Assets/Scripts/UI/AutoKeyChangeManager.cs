using System;

namespace Assets.Scripts.UI
{
    public class AutoKeyChangeManager
    {
        public static AutoKeyChangeManager Instance { get; } = new AutoKeyChangeManager();
        private AutoKeyChangeState _state = AutoKeyChangeState.None;
        public AutoKeyChangeState State => _state;
        public event Action<AutoKeyChangeState> OnStateChanged;

        public void SetState(AutoKeyChangeState newState)
        {
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
        public enum AutoKeyChangeState
        {
            None,
            Up,
            Down,
        }
    }

}
