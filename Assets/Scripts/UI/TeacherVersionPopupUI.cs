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
        private static readonly LocalizedMessage Message = new(
            japanese: "先生のアプリが古いバージョンです。アップデートを依頼してください。",
            english: "Your teacher's app is out of date. Please ask your teacher to update it.");

        [SerializeField] private VersionObserver _observer;

        private void Start()
        {
            _observer.OnPeerVersionOutdated
                .Where(isOutdated => isOutdated)
                .Subscribe(_ => ConfirmModalUI.Show(Message.ForCurrentLanguage()))
                .AddTo(this);
        }
    }
}
