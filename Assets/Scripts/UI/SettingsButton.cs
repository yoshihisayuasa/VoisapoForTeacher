using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace Assets.Scripts.UI
{
    public sealed class SettingsButton : MonoBehaviour
    {
        private const string SettingsModalResourcePath =
            "Prefab/UnityScreenNavigator/Modal/pfb_ui_modal_settings";

        public void OnClick()
        {
            ModalContainer.Find("ModalContainer").Push(SettingsModalResourcePath, true);
        }
    }
}
