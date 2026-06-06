using R3;

namespace Assets.Scripts.UI
{
    public class AutoKeyChangeManager
    {
        public static AutoKeyChangeManager Instance { get; } = new AutoKeyChangeManager();
        public ReactiveProperty<AutoKeyChangeState> State { get; } = new ReactiveProperty<AutoKeyChangeState>(AutoKeyChangeState.None);

        public void SetState(AutoKeyChangeState newState) => State.Value = newState;

        public void Toggle(AutoKeyChangeState target)
        {
            if (State.Value == target)
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
