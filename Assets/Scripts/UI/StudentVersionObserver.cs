using Photon.Pun;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class StudentVersionObserver : MonoBehaviourPunCallbacks
    {
        public const string StudentVersionKey = "isNewStudent";

        private readonly Subject<bool> _onStudentVersionOutdated = new();
        public Observable<bool> OnStudentVersionOutdated => _onStudentVersionOutdated;

        private void OnDestroy()
        {
            _onStudentVersionOutdated.Dispose();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            var isOutdated = !newPlayer.CustomProperties.ContainsKey(StudentVersionKey);
            _onStudentVersionOutdated.OnNext(isOutdated);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            _onStudentVersionOutdated.OnNext(false);
        }

    }
}
