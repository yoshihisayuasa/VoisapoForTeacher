using System;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// 音源側が交代した直後の再生を、少し待ってから始めるためのルール。
    /// 交代前の音は Zoom の伝送遅延ぶん遅れて相手に届くため、間を空けないと
    /// 前の音の鳴り終わりに次の再生が重なって聞こえる。
    ///
    /// 待ち時間は音源側・描画側の双方が同じ値で入れる（そうしないと音と鍵盤表示がズレる）。
    /// 判定材料は「自分が音源側か」だけで、この状態は両端末で反転同期されているため、
    /// それぞれが独立に判定しても必ず同じ結論になる。
    ///
    /// 時刻は引数で受け取り、Unity には依存しない（単体でテストできる）。
    /// </summary>
    public sealed class PlaybackHandoverPolicy
    {
        /// <summary>
        /// 音源側の交代直後に空ける最小間隔（秒）。
        /// 【暫定】デバッグ用に 2.0。リリース前に 0.3 へ戻すこと。
        /// エディタ・実機で値を変えてはいけない（両端末で食い違うと表示がズレる）。
        /// </summary>
        private const float HandoverGapSeconds = 2.0f;

        private bool _isSessionOpen;
        private bool _isCurrentSoundSide;

        private bool _wasPreviousSoundSide;

        // 起動直後は「前のセッションが無限に昔に終わった」とみなし、待ちを 0 にする。
        // これで「前のセッションが有るか」の分岐そのものが要らなくなる。
        private float _previousEndTime = float.NegativeInfinity;

        /// <summary>
        /// 再生セッションの開始を宣言し、開始前に空けるべき待ち時間（秒）を返す。
        /// 音源側が交代していなければ 0（通常操作の反応は落とさない）。
        /// </summary>
        public float BeginSession(bool isSoundSide, float now)
        {
            float gap = RequiredGap(isSoundSide, now);

            _isSessionOpen = true;
            _isCurrentSoundSide = isSoundSide;

            return gap;
        }

        /// <summary>
        /// 再生セッションの終了を記録する。次の交代はこの時刻から間隔を測る。
        /// 開いているセッションが無ければ何もしない（鳴っていない状態での停止操作で
        /// 終了時刻が更新され、間が空いているのに待たされるのを防ぐ）。
        /// </summary>
        public void EndSession(float now)
        {
            if (!_isSessionOpen)
            {
                return;
            }

            _isSessionOpen = false;
            _wasPreviousSoundSide = _isCurrentSoundSide;
            _previousEndTime = now;
        }

        private float RequiredGap(bool isSoundSide, float now)
        {
            if (isSoundSide == _wasPreviousSoundSide)
            {
                return 0f;
            }

            return Math.Max(0f, HandoverGapSeconds - (now - _previousEndTime));
        }
    }
}
