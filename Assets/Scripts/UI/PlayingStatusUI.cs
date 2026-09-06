using Assets.Scripts.UI.MelodyUI;
using R3;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 先生側で音が鳴っていることを知らせるステータステキスト。
    /// 相手が居ない間は「先生側」という区別そのものが成り立たないため、在室中だけ表示する。
    /// </summary>
    public sealed class PlayingStatusUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private ConnectionStatusObserver _observer;

        private bool _isTeacherSidePlaying;
        private bool _isParticipantPresent;

        private void Start()
        {
            _statusText.text = "Playing on teacher side";
            _statusText.color = AppColors.TeacherSide;
            _statusText.gameObject.SetActive(false);

            MelodyPlayer.Instance.OnTeacherPlayStatus
                .Subscribe(SetTeacherSidePlaying)
                .AddTo(this);

            MelodyPlayer.Instance.OnPlayEnded
                .Subscribe(_ => SetTeacherSidePlaying(false))
                .AddTo(this);

            _observer.OnParticipantJoined.Subscribe(_ => SetParticipantPresent(true)).AddTo(this);
            _observer.OnParticipantLeft.Subscribe(_ => SetParticipantPresent(false)).AddTo(this);
            _observer.OnSelfDisconnected.Subscribe(_ => SetParticipantPresent(false)).AddTo(this);
        }

        private void SetTeacherSidePlaying(bool isPlaying)
        {
            _isTeacherSidePlaying = isPlaying;
            UpdateVisibility();
        }

        private void SetParticipantPresent(bool isPresent)
        {
            _isParticipantPresent = isPresent;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            _statusText.gameObject.SetActive(_isTeacherSidePlaying && _isParticipantPresent);
        }
    }
}
