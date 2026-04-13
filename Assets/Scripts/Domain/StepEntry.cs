namespace AsseScripts.Domain
{
    public interface IStepEntry
    {
        string DisplayText { get; }
    }

    /// <summary>
    /// 音符ステップ。押した鍵の絶対音を保持する。
    /// インターバルへの変換は MelodyDraft.Build() で行う。
    /// </summary>
    public sealed class NoteStep : IStepEntry
    {
        public PianoNote Key { get; }

        public string DisplayText => Key.Note.ToString().Replace("Sharp", "#");

        public NoteStep(PianoNote key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// 延長ステップ。直前の NoteStep の拍数を +1 する。
    /// </summary>
    public sealed class ExtendStep : IStepEntry
    {
        public string DisplayText => "→";
    }
}
