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
            PersistentRegistry.DestroyAll();
            SceneManager.LoadScene(_roomIdSceneName);
        }
    }
}
