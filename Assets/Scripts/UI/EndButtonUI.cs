using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public sealed class EndButtonUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private string _roomIdSceneName = "RoomID";

        private void Start()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            RoomIdHolder.Clear();

            // イヤホンモードはアプリ寿命の状態なので、レッスンを終えるここで戻す。
            // 残すと次のレッスンに持ち越され、生徒が入室した瞬間に先生側で伴奏が鳴り始める。
            // 生徒の一時的な退室では戻さない。
            EarphoneModeManager.Instance.SetMode(false);

            PersistentRegistry.DestroyAll();
            SceneManager.LoadScene(_roomIdSceneName);
        }
    }
}
