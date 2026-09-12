using Assets.Scripts.Domain.StaticValues;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// マイクを開きっぱなしのリングバッファとして扱い、任意の区間だけを切り出す。
    ///
    /// フレーズごとに Microphone.Start / End を繰り返すと、そのたびにOSがオーディオデバイスを
    /// 開き直すため、再生中のピアノ音量が一瞬変わる。開閉は録音の有効化・無効化のときだけに限り、
    /// フレーズの区切りは書き込み位置を覚えるだけで表す。
    /// </summary>
    public sealed class MicrophoneCapture
    {
        private readonly string _deviceName;

        private AudioClip _buffer;
        private int _startPosition;
        private double _startTime;

        // 切り出しは周ごとに走る。リングバッファ全体（上限秒ぶん）の読み出し先は使い回す。
        private float[] _whole;

        public MicrophoneCapture(string deviceName)
        {
            _deviceName = deviceName;
        }

        /// <summary>同じマイクを指しているか。</summary>
        public bool IsSameDevice(string deviceName)
        {
            return _deviceName == deviceName;
        }

        /// <summary>マイクを開く。開けなければ false。</summary>
        public bool Open()
        {
            try
            {
                _buffer = Microphone.Start(_deviceName, true, RecordingRules.MaxPhraseSec, RecordingRules.SampleRate);
            }
            catch (System.Exception)
            {
                _buffer = null;
            }
            return _buffer != null;
        }

        public void Close()
        {
            Microphone.End(_deviceName);
            _buffer = null;
        }

        /// <summary>これ以降を1フレーズとして切り出す起点にする。</summary>
        public void MarkStart()
        {
            _startPosition = Microphone.GetPosition(_deviceName);
            _startTime = Time.realtimeSinceStartupAsDouble;
        }

        /// <summary>起点から現在までを切り出す。長さが無ければ null。</summary>
        public AudioClip ExtractSinceStart()
        {
            int total = _buffer.samples;
            int end = Microphone.GetPosition(_deviceName);
            int start = _startPosition;
            int length = end - start;
            if (length < 0)
            {
                length += total;
            }
            if (Time.realtimeSinceStartupAsDouble - _startTime >= RecordingRules.MaxPhraseSec)
            {
                // 上限を超えた分はリングバッファ上で上書き済みなので、直近の上限秒ぶんだけを残す
                start = end;
                length = total;
            }
            if (length <= 0)
            {
                return null;
            }
            return CreateClip(start, length, total);
        }

        private AudioClip CreateClip(int start, int length, int total)
        {
            int channels = _buffer.channels;
            if (_whole == null || _whole.Length != total * channels)
            {
                _whole = new float[total * channels];
            }
            _buffer.GetData(_whole, 0);

            var samples = new float[length * channels];
            for (int i = 0; i < length; i++)
            {
                int source = ((start + i) % total) * channels;
                for (int channel = 0; channel < channels; channel++)
                {
                    samples[i * channels + channel] = _whole[source + channel];
                }
            }

            var clip = AudioClip.Create(_buffer.name, length, channels, _buffer.frequency, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
