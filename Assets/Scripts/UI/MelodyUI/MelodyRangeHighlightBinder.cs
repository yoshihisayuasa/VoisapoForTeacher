using Assets.Scripts.UI.Piano;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyUI
{
    /// <summary>
    /// 選択鍵盤（根音）・選択メロディの変更に合わせて、再生可能範囲のハイライトを鍵盤へ反映する。
    /// 範囲ハイライトは演奏画面の先生専用機能のため、このコンポーネントは TeacherMain シーンに
    /// のみ配置し、生徒ビルドでは Awake で自身を破棄する（PianoKeyboardInput と同じパターン）。
    /// メロディ作成シーンには存在しないため、作成画面ではハイライトされない。
    /// </summary>
    public sealed class MelodyRangeHighlightBinder : MonoBehaviour
    {
        private readonly MelodyRangePresenter _rangePresenter = new();

        private void Awake()
        {
            if (!AppMode.IsTeacher)
            {
                // Destroy の実行はフレーム末のため、先に無効化して Start（購読開始）を走らせない。
                enabled = false;
                Destroy(this);
            }
        }

        private void Start()
        {
            var piano = PianoController.Instance;

            // 鍵盤は DontDestroyOnLoad で生き続けるため、シーン入場時に
            // 現在の選択・メロディで表示を作り直す（前回離脱時に消してある）。
            RefreshHighlight(piano);

            piano.OnSelectionChangedAsObservable
                .Subscribe(_ => RefreshHighlight(piano))
                .AddTo(this);

            MelodyManager.Instance.MelodyChanged
                .Subscribe(_ => RefreshHighlight(piano))
                .AddTo(this);
        }

        /// <summary>
        /// 鍵盤はシーンをまたいで残るため、シーン離脱（＝自身の破棄）時に
        /// 自分が表示した範囲ハイライトを消す。表示した者が片付ける。
        /// </summary>
        private void OnDestroy()
        {
            var piano = PianoController.Instance;
            if (piano == null) return; // アプリ終了時は鍵盤が先に破棄されていることがある

            piano.ClearHighlight();
        }

        private void RefreshHighlight(PianoController piano)
        {
            var melody = MelodyManager.Instance.CurrentMelody;
            if (melody == null) return;
            _rangePresenter.RefreshHighlight(piano, melody);
        }
    }
}
