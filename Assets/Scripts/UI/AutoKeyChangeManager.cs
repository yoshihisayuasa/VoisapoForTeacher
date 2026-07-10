using Assets.Scripts.Domain.ValueObjects;
using R3;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 自動キー変更の状態（先生の意図）を保持・通知する。
    /// 変更の入り口を「ローカル操作」と「ネットワーク受信」で分け、
    /// 送信側は LocalChanged だけを購読することで受信の再送信（エコー）を構造的に防ぐ。
    /// </summary>
    public class AutoKeyChangeManager
    {
        public static AutoKeyChangeManager Instance { get; } = new AutoKeyChangeManager();

        private readonly ReactiveProperty<AutoKeyChangeState> _state = new(AutoKeyChangeState.None);
        private readonly Subject<AutoKeyChangeState> _localChanged = new();

        public Observable<AutoKeyChangeState> State => _state;
        public AutoKeyChangeState Current => _state.Value;

        /// <summary>ローカル操作由来の変更のみ。ネットワーク送信はこちらを購読する。</summary>
        public Observable<AutoKeyChangeState> LocalChanged => _localChanged;

        public void SetState(AutoKeyChangeState newState)
        {
            _state.Value = newState;
            _localChanged.OnNext(newState);
        }

        public void Toggle(AutoKeyChangeState target)
        {
            if (_state.Value == target)
            {
                SetState(AutoKeyChangeState.None);
            }
            else
            {
                SetState(target);
            }
        }

        /// <summary>受信した状態の適用。LocalChanged には流れないため再送信されない。</summary>
        public void ApplyRemote(AutoKeyChangeState newState)
        {
            _state.Value = newState;
        }
    }
}
