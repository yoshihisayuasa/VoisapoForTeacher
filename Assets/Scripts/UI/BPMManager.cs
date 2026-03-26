using AsseScripts.Domain;
using System;

namespace AsseScripts.UI
{
    public class BPMManager
    {
        public static BPMManager Instance { get; } = new BPMManager();

        private BPM _bpm = new BPM(120);
        public event Action<int> BpmChanged;

        public float SecondPerBeat => _bpm.SecondPerBeat;
        public int Value => _bpm.Value;


        public void Increment(int step = 10)
        {
            _bpm = _bpm.Increment(step);
            BpmChanged?.Invoke(_bpm.Value);
        }
        public void Decrement(int step = 10)
        {
            _bpm = _bpm.Decrement(step);
            BpmChanged?.Invoke(_bpm.Value);
        }

    }
}