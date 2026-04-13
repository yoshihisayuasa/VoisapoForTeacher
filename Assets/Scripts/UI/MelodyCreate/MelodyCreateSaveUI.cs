using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// メロディ名の入力と保存/キャンセルボタンを管理する。
    /// </summary>
    public sealed class MelodyCreateSaveUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameField;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _cancelButton;

        private void Start()
        {
            _nameField.onValueChanged.AddListener(MelodyCreateManager.Instance.SetName);
            _saveButton.onClick.AddListener(MelodyCreateManager.Instance.Save);
            _cancelButton.onClick.AddListener(MelodyCreateManager.Instance.Cancel);

            MelodyCreateManager.Instance.DraftChanged += RefreshSaveButton;
            RefreshSaveButton();
        }

        private void OnDestroy()
        {
            if (MelodyCreateManager.Instance != null)
            {
                MelodyCreateManager.Instance.DraftChanged -= RefreshSaveButton;
            }
        }

        private void RefreshSaveButton()
        {
            _saveButton.interactable = MelodyCreateManager.Instance.IsDraftValid;
        }
    }
}
