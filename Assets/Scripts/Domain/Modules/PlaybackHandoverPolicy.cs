using System;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// 音源側が交代した直後の再生を、少し待ってから始めるためのルール。
    /// 交代前の音は Zoom の伝送遅延ぶん遅れて相手に届くため、間を空けないと
    /// 前の音の鳴り終わりに次の再生が重なって聞こえる。
    ///
    /// 待ち時間は音源側・描画側の双方が同じ値で入れる（そうしないと音と鍵盤表示がズレる）。
    /// 判定材料は「そのセッションで自分が音源側だったか」だけで、これは再生権限トークンの
    /// 有無そのもの。トークンは常にどちらか一方だけが持ち、受け渡しは周境界のバトンで
    /// 起きるため、それぞれが独立に判定しても必ず同じ結論になる。
    /// 「先生が選んだ再生側」のフラグを渡してはいけない。トグル操作の瞬間に変わるので、
    /// 実際の交代（バトン）より先行して食い違う。
    ///
    /// 時刻は引数で受け取り、Unity には依存しない（単体でテストできる）。
    /// </summary>
    public sealed class PlaybackHandoverPolicy
    {
        /// <summary>
        /// 音源側の交代直後に空ける最小間隔（秒）。
        /// 間の長さを目で追えるよう、デバッグ中だけ長めに取る。
        ///
        /// 両端末で必ず同じ値になっていなければならない（食い違うと音と鍵盤表示がズレる）。
        /// そのため切り替えはエディタ設定ではなくビルド設定に紐づけてある。
        /// デバッグ時は生徒アプリも Development Build でビルドすること
        /// （先生＝エディタ／生徒＝リリースビルド の組み合わせでは値が食い違う）。
        /// </summary>
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private const float HandoverGapSeconds = 2.0f;
#else
        private const float HandoverGapSeconds = 0.3f;
#endif

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
