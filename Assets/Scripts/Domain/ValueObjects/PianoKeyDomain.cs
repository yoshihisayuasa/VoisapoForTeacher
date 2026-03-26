
namespace AsseScripts.Domain
{
    /// <summary>
    /// 鍵盤の状態・イベント管理（ドメイン層）
    /// </summary>
    public class PianoKeyDomain
    {
        public PianoNote Key { get; }
        public PianoKeyDomain(PianoNoteEnum keyEnum)
        {
            Key = new PianoNote(keyEnum);
        }
    }
}