using Assets.Scripts.Domain.Modules;
using System.Collections.Generic;

namespace Assets.Scripts.Domain.ValueObjects
{
    /// <summary>
    /// メロディの再生種別。種別ごとに決まること（保存データ上の名前・目印の出し方）は種別自身が持つ。
    /// インスタンスはここに並ぶものだけで、外からは作れない。
    /// </summary>
    public sealed class MelodyKind
    {
        /// <summary>和音に続けてメロディを1音ずつ鳴らす。自動転調に対応する唯一の種別。</summary>
        public static readonly MelodyKind Standard = new("Standard", new RangeHighlightRule());

        /// <summary>押している間だけ根音を鳴らす。</summary>
        public static readonly MelodyKind Single = new("Single", new RangeHighlightRule());

        /// <summary>和音を鳴らしたあと、停止されるまでメトロノームだけを刻み続ける。</summary>
        public static readonly MelodyKind ChordWithMetronome = new("ChordWithMetronome", new OctaveHighlightRule());

        /// <summary>和音だけを既定の拍数ぶん鳴らして終わる。</summary>
        public static readonly MelodyKind Chord = new("Chord", new OctaveHighlightRule());

        // 上の静的フィールドより後ろに置くこと（初期化は記述順のため、前に置くと null が入る）。
        private static readonly IReadOnlyList<MelodyKind> All = new[]
        {
            Standard,
            Single,
            ChordWithMetronome,
            Chord,
        };

        private readonly IHighlightRule _highlightRule;

        /// <summary>
        /// 保存データ・ネットワーク送受信で種別を表す名前。保存フォーマットの一部なので、
        /// フィールド名を変えてもこの文字列は変えない（変えると既存データが読めなくなる）。
        /// </summary>
        public string SavedName { get; }

        private MelodyKind(string savedName, IHighlightRule highlightRule)
        {
            SavedName = savedName;
            _highlightRule = highlightRule;
        }

        /// <summary>
        /// 保存名から種別を復元する。名前の完全一致だけを受け付け、
        /// 欠損・不明な値は Standard（通常再生）として扱う。
        /// </summary>
        public static MelodyKind Parse(string savedName)
        {
            foreach (var kind in All)
            {
                if (kind.SavedName == savedName)
                {
                    return kind;
                }
            }
            return Standard;
        }

        /// <summary>
        /// この種別のメロディが鍵盤に出す目印の上端を、音域の最低音・最高音（根音からのインターバル）から決める。
        /// </summary>
        public Interval HighlightTop(Interval lowest, Interval highest)
        {
            return _highlightRule.Top(lowest, highest);
        }
    }
}
