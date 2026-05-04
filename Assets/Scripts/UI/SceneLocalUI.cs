using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI
{
    public sealed class SceneLocalUI : MonoBehaviour
    {
        [SerializeField] private string _visibleSceneName = "Main";

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            gameObject.SetActive(scene.name == _visibleSceneName);
        }
    }
}
