using Assets.Scripts.Domain.ValueObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// 役割：選択鍵盤（根音）とハイライト範囲（Min/Max）の視覚状態管理。
    /// 選択とハイライトが同じ鍵盤に重なったときの色の復元も、このクラスが解決する。
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
                GetKeyUI(_selectedKey).ResetAccentColor();
                RestoreHighlightIfNeeded(_selectedKey);
            }
            _selectedKey = key;
            GetKeyUI(_selectedKey).SetAccentColor();
        }

        /// <summary>
        /// 選択中の鍵盤を delta だけ移動し、移動先の鍵盤を返す。
        /// </summary>
        public PianoNote MoveSelection(int delta)
        {
            Debug.Assert(_selectedKey != null, "_selectedKey is null");
            var nextKey = _selectedKey.MovedBy(delta, _keys.Count);

            SelectKey(nextKey);
            return nextKey;
        }

        public void Deselect()
        {
            if (_selectedKey == null) return;
            GetKeyUI(_selectedKey).ResetAccentColor();
            RestoreHighlightIfNeeded(_selectedKey);
            _selectedKey = null;
        }

        public void SetHighlight(PianoKeyRange range)
        {
            ClearHighlight();

            _highlightedRange = range;

            if (range.Min != _selectedKey) GetKeyUI(range.Min).SetMinHighlightColor();
            if (range.Max != _selectedKey) GetKeyUI(range.Max).SetMaxHighlightColor();
        }

        /// <summary>
        /// 現在のハイライト範囲（Min/Max）を解除する。再生可能範囲外のキーを選択したときに呼ぶ。
        /// </summary>
        public void ClearHighlight()
        {
            if (_highlightedRange is null) return;

            var oldMin = _highlightedRange.Min;
            var oldMax = _highlightedRange.Max;
            if (oldMin != _selectedKey) GetKeyUI(oldMin).ResetHighlightedColor();
            if (oldMax != _selectedKey) GetKeyUI(oldMax).ResetHighlightedColor();

            _highlightedRange = null;
        }

        private void RestoreHighlightIfNeeded(PianoNote key)
        {
            if (_highlightedRange is null)
            {
                return;
            }
            if (_highlightedRange.IsMin(key))
            {                 
                GetKeyUI(key).SetMinHighlightColor();
            }
            else if (_highlightedRange.IsMax(key))
            { 
                GetKeyUI(key).SetMaxHighlightColor();
            }
        }

        private PianoKeyUI GetKeyUI(PianoNote key)
        {
            return _keys[key.Note];
        }
    }
}
