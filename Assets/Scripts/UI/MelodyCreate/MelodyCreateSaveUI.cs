using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.MelodyCreate
{
    public sealed class MelodyCreateSaveUI : MonoBehaviour
    {
        [SerializeField] private Button _saveButton;
        [SerializeField] private MelodyNameInputPopupUI _popup;

        private void Start()
        {
            _saveButton.onClick.AddListener(_popup.Open);

            MelodyCreateManager.Instance.DraftChanged
                .Subscribe(_ => RefreshSaveButton())
                .AddTo(this);
            RefreshSaveButton();
        }

        private void RefreshSaveButton()
        {
            _saveButton.interactable = MelodyCreateManager.Instance.CanPreview;
        }
    }
}
