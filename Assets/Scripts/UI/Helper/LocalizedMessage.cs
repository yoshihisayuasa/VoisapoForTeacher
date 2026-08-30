using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 日本語と英語の文言を一組で持つ不変の値。
    /// 端末の言語設定による出し分けをここ1箇所に閉じ込め、表示側から言語判定をなくす。
    /// </summary>
    public sealed class LocalizedMessage
    {
        private readonly string _japanese;
        private readonly string _english;

        public LocalizedMessage(string japanese, string english)
        {
            _japanese = japanese;
            _english = english;
        }

        /// <summary>端末の言語設定に合う文言。日本語以外の言語はすべて英語で表示する。</summary>
        public string ForCurrentLanguage()
        {
            return Application.systemLanguage == SystemLanguage.Japanese
                ? _japanese
                : _english;
        }
    }
}
