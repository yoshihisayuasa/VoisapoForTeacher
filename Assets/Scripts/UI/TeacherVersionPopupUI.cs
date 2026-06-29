using Assets.Scripts.UI.Modal;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 先生のアプリバージョンが必要バージョン未満のとき、生徒にポップアップで警告する。
    /// </summary>
    public sealed class TeacherVersionPopupUI : MonoBehaviour
    {
        [SerializeField] private VersionObserver _observer;

        [SerializeField]
        private string _message = "先生のアプリが古いバージョンです。アップデートを依頼してください。";

        private void Start()
        {
            _observer.OnPeerVersionOutdated
                .Where(isOutdated => isOutdated)
                .Subscribe(_ => ConfirmModalUI.Show(_message))
                .AddTo(this);
        }
    }
}
