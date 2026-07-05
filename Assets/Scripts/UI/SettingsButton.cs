using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace Assets.Scripts.UI
{
    public sealed class SettingsButton : MonoBehaviour
    {
        [SerializeField]
        private string _settingsModalResourcePath;

        public void OnClick()
        {
            ModalContainer.Find("ModalContainer").Push(_settingsModalResourcePath, true);
        }
    }
}
