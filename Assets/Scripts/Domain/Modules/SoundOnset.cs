using Assets.Scripts.Domain.StaticValues;
using System;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// 録音の中で音が鳴り始める位置を探す。
    ///
    /// 録音の起点はアプリが鳴らし始めた瞬間だが、音がスピーカーから出てマイクに入るまでには遅れがある。
    /// その遅れぶん録音の頭は無音になり、聴き直すと鍵盤の動きが声より先に進む。
    /// 先生の端末で鳴らした伴奏はマイクに回り込んで録音に入っているので、その出だしを探せば
    /// 遅れを実測できる（環境ごとに違う遅れを決め打ちしなくて済む）。
    ///
    /// Unity には依存しない（単体でテストできる）。
    /// </summary>
    public static class SoundOnset
    {
        /// <summary>
        /// 音が鳴り始めるフレーム位置を返す。見つからなければ 0（頭を削らない）。
        /// samples はチャンネルを交互に並べたもの。
        /// </summary>
        public static int FindFrame(float[] samples, int channels, int frequency)
        {
            float peak = PeakOf(samples);
            if (peak < RecordingRules.SilenceLevel)
            {
                return 0;
            }
            float threshold = peak * RecordingRules.OnsetThresholdRatio;

            int frames = samples.Length / channels;
            int searchFrames = Math.Min(frames, (int)(RecordingRules.OnsetSearchSec * frequency));
            int preRollFrames = (int)(RecordingRules.OnsetPreRollSec * frequency);
            for (int frame = 0; frame < searchFrames; frame++)
            {
                for (int channel = 0; channel < channels; channel++)
                {
                    if (Math.Abs(samples[frame * channels + channel]) >= threshold)
                    {
                        return Math.Max(0, frame - preRollFrames);
                    }
                }
            }
            return 0;
        }

        private static float PeakOf(float[] samples)
        {
            float peak = 0f;
            foreach (float sample in samples)
            {
                peak = Math.Max(peak, Math.Abs(sample));
            }
            return peak;
        }
    }
}
