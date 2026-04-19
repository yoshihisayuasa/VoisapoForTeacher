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
        [SerializeField] private DraftMelodyPlayer _melodyPlayer;

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
            var manager = MelodyCreateManager.Instance;
            _melodyPlayer.Play(manager.CurrentMelody, manager.Draft.Root);
        }



        private void Refresh()
        {
            _previewButton.interactable = MelodyCreateManager.Instance.Draft.CanPreview;
        }
    }
}
