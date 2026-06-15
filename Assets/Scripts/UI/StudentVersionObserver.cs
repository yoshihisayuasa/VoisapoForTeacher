using AsseScripts.Domain;
using Photon.Pun;
using Photon.Realtime;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class StudentVersionObserver : MonoBehaviourPunCallbacks
    {
        public const string StudentVersionKey = "studentVersion";
        private static readonly AppVersion MinRequiredVersion = new("1.0.0");

        private readonly Subject<bool> _onStudentVersionOutdated = new();
        public Observable<bool> OnStudentVersionOutdated => _onStudentVersionOutdated;

        private void OnDestroy()
        {
            _onStudentVersionOutdated.Dispose();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            _onStudentVersionOutdated.OnNext(IsOutdated(newPlayer));
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            _onStudentVersionOutdated.OnNext(false);
        }

        private static bool IsOutdated(Player player)
        {
            if (!player.CustomProperties.TryGetValue(StudentVersionKey, out var raw))
                return true;
            if (!AppVersion.TryCreate(raw.ToString(), out var studentVersion))
                return true;
            return MinRequiredVersion.IsNewerThan(studentVersion);
        }
    }
}
