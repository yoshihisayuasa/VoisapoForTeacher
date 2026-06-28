using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace Assets.Scripts.UI.MelodyCreate
{
    public sealed class MelodyNameInputPopupUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameInputField;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _cancelButton;

        private void Start()
        {
            _okButton.onClick.AddListener(OnOkButtonClicked);
            _cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        private void OnOkButtonClicked()
        {
            MelodyCreateManager.Instance.SaveWithName(_nameInputField.text);
        }

        private void OnCancelButtonClicked()
        {
            ModalContainer.Of(transform).Pop(true);
        }
    }
}
