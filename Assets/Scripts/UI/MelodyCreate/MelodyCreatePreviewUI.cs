using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// メロディ作成シーンのプレビューボタン。
    /// 下書きに和音とメロディ音が揃ったら有効化し、押下で DraftMelodyPlayer に再生を委譲する。
    /// </summary>
    public sealed class MelodyCreatePreviewUI : MonoBehaviour
    {
        [SerializeField] private Button _previewButton;

        private void Start()
        {
            _previewButton.onClick.AddListener(OnClicked);
            MelodyCreateManager.Instance.DraftChanged
                .Subscribe(_ => Refresh())
                .AddTo(this);
            Refresh();
        }

        private void OnClicked()
        {
            MelodyCreateManager.Instance.Preview();
        }

        private void Refresh()
        {
            _previewButton.interactable = MelodyCreateManager.Instance.CanPreview;
        }
    }
}
