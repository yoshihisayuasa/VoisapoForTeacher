using R3;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace Assets.Scripts.UI.MelodyCreate
{
    public sealed class MelodyCreateSaveUI : MonoBehaviour
    {
        private const string NameInputModalResourcePath =
            "Prefab/UnityScreenNavigator/Modal/pfb_ui_modal_melody_name_input";

        [SerializeField] private Button _saveButton;

        private void Start()
        {
            _saveButton.onClick.AddListener(OpenNameInputModal);

            MelodyCreateManager.Instance.DraftChanged
                .Subscribe(_ => RefreshSaveButton())
                .AddTo(this);
            RefreshSaveButton();
        }

        private void OpenNameInputModal()
        {
            ModalContainer.Find("ModalContainer").Push(NameInputModalResourcePath, true);
        }

        private void RefreshSaveButton()
        {
            _saveButton.interactable = MelodyCreateManager.Instance.CanPreview;
        }
    }
}
