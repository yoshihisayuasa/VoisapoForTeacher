using Photon.Pun;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class ConnectionStatusObserver : MonoBehaviourPunCallbacks
    {
        private readonly Subject<Unit> _onStudentJoined = new();
        public Observable<Unit> OnStudentJoined => _onStudentJoined;

        private readonly Subject<Unit> _onStudentLeft = new();
        public Observable<Unit> OnStudentLeft => _onStudentLeft;

        private readonly Subject<Unit> _onTeacherDisconnected = new();
        public Observable<Unit> OnTeacherDisconnected => _onTeacherDisconnected;

        private void OnDestroy()
        {
            _onStudentJoined.Dispose();
            _onStudentLeft.Dispose();
            _onTeacherDisconnected.Dispose();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            _onStudentJoined.OnNext(Unit.Default);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            _onStudentLeft.OnNext(Unit.Default);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            _onTeacherDisconnected.OnNext(Unit.Default);
        }
    }
}
