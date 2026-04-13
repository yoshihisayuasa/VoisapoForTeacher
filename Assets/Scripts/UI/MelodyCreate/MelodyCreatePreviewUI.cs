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
        [SerializeField] private DraftMelodyPlayer _melodyPlayer;

        private void Start()
        {
            _previewButton.onClick.AddListener(OnClicked);
            MelodyCreateManager.Instance.DraftChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (MelodyCreateManager.Instance != null)
            {
                MelodyCreateManager.Instance.DraftChanged -= Refresh;
            }
        }

        private void OnClicked()
        {
            _melodyPlayer.Play(MelodyCreateManager.Instance.Draft);
        }

        private void Refresh()
        {
            _previewButton.interactable = MelodyCreateManager.Instance.Draft.CanPreview;
        }
    }
}
