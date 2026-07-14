using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;

namespace Assets.Scripts.UI.MelodyUI
{
    /// <summary>
    /// 選択中メロディの再生可能範囲を鍵盤表示へ反映する表示専任クラス。
    /// ハイライトの更新（MelodyRangeHighlightBinder＝演奏シーンの先生専用）と、
    /// 範囲を画面内へ収めるスクロール（MelodyPlayer＝全ビルド共通）を担う。
    /// </summary>
    public sealed class MelodyRangePresenter
    {
        public void RefreshHighlight(PianoController piano, Melody melody)
        {
            var rootKey = piano.SelectedKey;
            if (rootKey == null || !melody.IsPlayableAt(rootKey, piano.KeyCount))
            {
                piano.ClearHighlight();
                return;
            }
            piano.SetHighlight(melody.KeyRangeAt(rootKey));
        }

        public void EnsureVisible(PianoController piano, Melody melody)
        {
            var rootKey = piano.SelectedKey;
            if (rootKey == null || !melody.IsPlayableAt(rootKey, piano.KeyCount))
            {
                return;
            }
            piano.EnsureRangeVisible(melody.KeyRangeAt(rootKey));
        }
    }
}
