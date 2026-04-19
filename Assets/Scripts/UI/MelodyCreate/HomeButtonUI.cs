using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.MelodyCreate
{
    public sealed class HomeButtonUI : MonoBehaviour
    {
        [SerializeField] private Button _homeButton;

        private void Start()
        {
            _homeButton.onClick.AddListener(MelodyCreateManager.Instance.Home);
        }
    }
}
