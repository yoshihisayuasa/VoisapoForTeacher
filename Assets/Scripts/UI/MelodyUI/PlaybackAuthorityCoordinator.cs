using Assets.Scripts.Domain.ValueObjects;
using R3;

namespace Assets.Scripts.UI.MelodyUI
{
    /// <summary>
    /// 再生権限トークンとバトン（委譲）の調停だけを担う。
    /// コルーチンや鍵盤操作といった「再生の実行」は持たず、状態遷移と判断のみに徹する。
    /// このため MonoBehaviour に依存せず、単体でテストできる。
    /// </summary>
    public sealed class PlaybackAuthorityCoordinator
    {
        // トークン保持者（音源側）だけがループ・周キー送信・±2即時反転の判断を行う。
        // 非保持者（描画側）は受信したキーごとの1回再生に徹する。
        public bool HasToken { get; private set; }

        // 描画中の周を弾き切ってから引き継ぐための保留キー。
        private PianoNote _pendingBaton;
        public bool HasPendingBaton => _pendingBaton != null;

        private readonly Subject<PianoNote> _onBatonPassed = new();

        /// <summary>再生権限を委譲した。次の権威が再生を始めるキーを運ぶ（ローカル発のみ）。</summary>
        public Observable<PianoNote> OnBatonPassed => _onBatonPassed;

        /// <summary>再生開始・再スタート時に、この端末がトークンを持つかを確定する。</summary>
        public void AssumeToken(bool hasToken)
        {
            HasToken = hasToken;
        }

        /// <summary>再生の停止に合わせて権限状態を初期化する（トークン返上・保留破棄）。</summary>
        public void Clear()
        {
            HasToken = false;
            _pendingBaton = null;
        }

        /// <summary>バトンを受け取り、引き継ぎ待ちに積む。昇格の契機は呼び出し側が選ぶ。</summary>
        public void SetPending(PianoNote nextKey)
        {
            _pendingBaton = nextKey;
        }

        /// <summary>
        /// ループ境界で、もう音源側でなければトークンを返上してバトンを渡す。
        /// 「停止」と「委譲」は境界での不可分な1処理（分けると渡し損ねの中間状態が生まれる）。
        /// 委譲したら true を返し、呼び出し側はそのまま再生を打ち切る。
        /// </summary>
        public bool TryPassBatonAtBoundary(bool isSoundPlay, PianoNote currentKey)
        {
            if (isSoundPlay)
            {
                return false;
            }

            HasToken = false;
            _onBatonPassed.OnNext(currentKey);
            return true;
        }

        /// <summary>
        /// 保留中のバトンがあればトークン保持者へ昇格し、再開キーを返す。
        /// 実際の再生開始（鍵盤操作・コルーチン起動）は呼び出し側が行う。
        /// </summary>
        public bool TryPromote(out PianoNote startKey)
        {
            if (_pendingBaton == null)
            {
                startKey = null;
                return false;
            }

            startKey = _pendingBaton;
            _pendingBaton = null;
            HasToken = true;
            return true;
        }
    }
}
