using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.UI.MelodyUI
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
            MelodyPlayer.Instance.StopMelody(true, shouldDelayRecordStop: false);
            MelodyManager.Instance.ClearCurrentMelody();
            AutoKeyChangeManager.Instance.SetState(AutoKeyChangeManager.AutoKeyChangeState.None);
            EarphoneModeManager.Instance.SetMode(false);
            SceneManager.LoadScene(_melodyCreateSceneName);
        }
    }
}
