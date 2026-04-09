namespace AsseScripts.Domain
{
    public abstract class StepEntry { }

    /// <summary>
    /// 音符ステップ。押した鍵の絶対音を保持する。
    /// インターバルへの変換は MelodyDraft.Build() で行う。
    /// </summary>
    public sealed class NoteStep : StepEntry
    {
        public PianoNote Key { get; set; }

        public NoteStep(PianoNote key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// 延長ステップ。直前の NoteStep の拍数を +1 する。
    /// </summary>
    public sealed class ExtendStep : StepEntry { }
}
