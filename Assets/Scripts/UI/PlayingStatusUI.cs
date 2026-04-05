using Assets.Scripts.UI.Melody;
using R3;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class PlayingStatusUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;

        private void Start()
        {
            _statusText.gameObject.SetActive(false);

            MelodyPlayer.Instance.OnPlayBegan
                .Subscribe(isTeacherSide =>
                {
                    _statusText.text = isTeacherSide ? "Playing on teacher side" : "Playing on student side";
                    _statusText.color = isTeacherSide ? AppColors.TeacherSide : AppColors.StudentSide;
                    _statusText.gameObject.SetActive(true);
                })
                .AddTo(this);

            MelodyPlayer.Instance.OnPlayEnded
                .Subscribe(_ => _statusText.gameObject.SetActive(false))
                .AddTo(this);
        }
    }
}
