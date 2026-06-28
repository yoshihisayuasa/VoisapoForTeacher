using AsseScripts.Domain;
using R3;

namespace AsseScripts.UI
{
    public sealed class BPMManager
    {
        public static BPMManager Instance { get; } = new BPMManager();

        private BPM _bpm = new(120);
        private readonly Subject<int> _bpmChanged = new();
        public Observable<int> BpmChanged => _bpmChanged;

        public float SecondPerBeat => _bpm.SecondPerBeat;
        public int Value => _bpm.Value;


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