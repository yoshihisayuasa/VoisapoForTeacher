using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// 1フレーズの録音の起点。起点ごとに値として持つので、締めを待っている間に次のフレーズが
    /// 起点を取り直しても、待っている側の起点は上書きされない。
    /// </summary>
    public sealed class CaptureStart
    {
        private readonly MicrophoneCapture _capture;
        private readonly int _position;
        private readonly double _time;

        internal CaptureStart(MicrophoneCapture capture, int position, double time)
        {
            _capture = capture;
            _position = position;
            _time = time;
        }

        /// <summary>起点から現在までを切り出す。長さが無ければ null。</summary>
        public AudioClip ExtractUntilNow()
        {
            return _capture.ExtractSince(_position, _time);
        }
    }
}
