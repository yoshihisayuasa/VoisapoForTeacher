using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Melody
{
    /// <summary>
    /// メロディ作成シーンへ遷移するボタン。
    /// </summary>
    public sealed class NewMelodyButton : MonoBehaviour
    {
        [SerializeField] private string _melodyCreateSceneName = "MelodyCreate";

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            SceneManager.LoadScene(_melodyCreateSceneName);
        }
    }
}
