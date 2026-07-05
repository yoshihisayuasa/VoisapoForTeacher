using Assets.Scripts.UI.MelodyUI;
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

            MelodyPlayer.Instance.OnTeacherPlayStatus
                .Subscribe(isTeacherSidePlaying =>
                {
                    if (isTeacherSidePlaying)
                    {
                        _statusText.text = "Playing on teacher side";
                        _statusText.color = AppColors.TeacherSide;
                    }
                    _statusText.gameObject.SetActive(isTeacherSidePlaying);
                })
                .AddTo(this);

            MelodyPlayer.Instance.OnPlayEnded
                .Subscribe(_ => _statusText.gameObject.SetActive(false))
                .AddTo(this);
        }
    }
}
