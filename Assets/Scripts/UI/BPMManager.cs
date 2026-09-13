using Assets.Scripts.Domain.ValueObjects;
using R3;

namespace Assets.Scripts.UI
{
    public sealed class BPMManager
    {
        public static BPMManager Instance { get; } = new BPMManager();

        private BPM _bpm = new(120);
        private readonly Subject<int> _bpmChanged = new();
        public Observable<int> BpmChanged => _bpmChanged;

        public float SecondPerBeat => _bpm.SecondPerBeat;
        public float KeyFadeOutSeconds => _bpm.KeyFadeOutSeconds;
        public int Value => _bpm.Value;

        /// <summary>
        /// 現在のテンポ。あとで同じ速さを再現する側（録音など）が、その時点の値を
        /// 持ち帰るために使う。不変なので、以降ここが変わっても持ち帰った値は動かない。
        /// </summary>
        public BPM Current => _bpm;


        public void Increment(int step = 10)
        {
            _bpm = _bpm.Increment(step);
            _bpmChanged.OnNext(_bpm.Value);
        }
        public void Decrement(int step = 10)
        {
            _bpm = _bpm.Decrement(step);
            _bpmChanged.OnNext(_bpm.Value);
        }

        /// <summary>
        /// 絶対値でBPMを設定する。先生からの同期受信に使う。
        /// </summary>
        public void SetValue(int value)
        {
            _bpm = new BPM(value);
            _bpmChanged.OnNext(_bpm.Value);
        }

    }
}