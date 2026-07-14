using Assets.Scripts.Domain.ValueObjects;
using System.Collections.Generic;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// 役割：選択鍵盤（根音）とハイライト範囲（Min/Max）の視覚状態管理。
    /// 選択とハイライトが同じ鍵盤に重なったときの色の優先順位は PianoKeyUI が解決する。
    /// </summary>
    public sealed class PianoKeyHighlighter
    {
        private readonly IReadOnlyDictionary<PianoNoteEnum, PianoKeyUI> _keys;
        private PianoNote _selectedKey;
        private PianoKeyRange _highlightedRange;

        public PianoKeyHighlighter(IReadOnlyDictionary<PianoNoteEnum, PianoKeyUI> keys)
        {
            _keys = keys;
        }

        public PianoNote SelectedKey => _selectedKey;

        public void SelectKey(PianoNote key)
        {
            if (_selectedKey != null)
            {
                GetKeyUI(_selectedKey).Deselect();
            }
            _selectedKey = key;
            GetKeyUI(_selectedKey).Select();
        }

        public void Deselect()
        {
            if (_selectedKey == null) return;
            GetKeyUI(_selectedKey).Deselect();
            _selectedKey = null;
        }

        public void SetHighlight(PianoKeyRange range)
        {
            ClearHighlight();

            _highlightedRange = range;

            GetKeyUI(range.Min).MarkAsRangeMin();
            GetKeyUI(range.Max).MarkAsRangeMax();
        }

        /// <summary>
        /// 現在のハイライト範囲（Min/Max）を解除する。再生可能範囲外のキーを選択したときに呼ぶ。
        /// </summary>
        public void ClearHighlight()
        {
            if (_highlightedRange is null) return;

            GetKeyUI(_highlightedRange.Min).ClearRangeEdge();
            GetKeyUI(_highlightedRange.Max).ClearRangeEdge();

            _highlightedRange = null;
        }

        private PianoKeyUI GetKeyUI(PianoNote key)
        {
            return _keys[key.Note];
        }
    }
}
