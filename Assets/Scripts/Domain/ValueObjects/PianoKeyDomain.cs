
namespace Scripts.Domain
{
    /// <summary>
    /// 鍵盤の状態・イベント管理（ドメイン層）
    /// </summary>
    public class PianoKeyDomain
    {
        public PianoNote Key { get; }
        public bool IsPressed { get; private set; }

        public PianoKeyDomain(PianoNoteEnum keyEnum)
        {
            Key = new PianoNote(keyEnum);
        }

        public void Press()
        {
            IsPressed = true;
        }

        public void Release()
        {
            IsPressed = false;
        }
    }
}