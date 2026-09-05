using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// 標準再生：和音パートに続けてメロディパートを1音ずつ鳴らす。
    /// 自動転調に対応する唯一の種別で、Execute はセッション全体（周ごとの転調ループ）を回す。
    /// </summary>
    public sealed class StandardPlayStrategy : MelodyPlayStrategy
    {
        public StandardPlayStrategy(IMelodyPlaybackContext context) : base(context)
        {
        }

        public override IEnumerator Execute(PianoController piano, Melody melody,
                                            PianoNote pressedKey, PlayModeSettings settings)
        {
            // 1周目。開始時の NotifyTeacherPlayStatus は呼び出し側（PlaySession）が済ませている。
            yield return PlayOnePass(piano, melody, pressedKey, settings);

            // 自動転調ループはトークン保持者（音源側）だけが回す。描画側はここに入らず1周で終わる。
            //
            // 周のルートは Context が持つものだけを使い、piano.SelectedKey は読まない。
            // SelectedKey は先生のマウスホバーでも動く「表示上の選択」で、鍵盤を横切っただけで
            // 変わる。これを読み戻すと、転調先もバトンに載せるキーもカーソル位置に飛ぶ。
            while (Context.ShouldContinueAutoKeyChange())
            {
                piano.StopAllKeys(true);

                // 権限委譲: 自分がもう音源側でないなら、弾き切ったこの境界で止めて渡す。
                // 新権威は同じキーから再開する（サイド切替の周は移調しない、従来の挙動を維持）。
                // 渡したらこのセッションは終了。FinishMelody は呼ばず（＝描画側に転じる）抜ける。
                if (Context.TryPassBatonAtBoundary())
                {
                    yield break;
                }

                var nextKey = Context.NextAutoKeyChangeRoot();

                if (!melody.IsPlayableAt(nextKey, piano.KeyCount))
                {
                    break;
                }

                Context.AdvanceTo(nextKey);
                Context.NotifyTeacherPlayStatus();

                yield return PlayOnePass(piano, melody, nextKey, settings);
            }

            // 自然終了（音域端 or 転調が無効／トークン喪失）。保留バトンがあれば終了せず抜け、
            // RunPlayback 側で権威を引き継ぐ（FinishMelody は保留破棄を伴うためここでは呼ばない）。
            if (!Context.HasPendingBaton)
            {
                Context.FinishMelody();
            }
        }

        // 離鍵では止めない（自動転調ループ／鳴り切りに委ねる）。
        public override void OnKeyUp()
        {
        }

        // 和音パート → メロディパートを開始キーで1周鳴らす。
        private IEnumerator PlayOnePass(PianoController piano, Melody melody,
                                        PianoNote key, PlayModeSettings settings)
        {
            // ── 和音パート ──
            Context.BeginChordSection();
            yield return PlayChordOnce(piano, melody.ChordAt(key), settings);
            Context.EndChordSection();

            // ── メロディパート ──
            Context.NotifyMelodyBegan();
            foreach (var note in melody.NotesAt(key))
            {
                piano.Play(note.Key, settings.PlayPiano);
                yield return new WaitForSeconds(BPMManager.Instance.SecondPerBeat * note.Beats);
                piano.StopAndMarkPlayed(note.Key);
            }
        }
    }
}
