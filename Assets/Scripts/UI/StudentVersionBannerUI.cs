using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class StudentVersionBannerUI : MonoBehaviour
    {
        [SerializeField] private GameObject _bannerRoot;
        [SerializeField] private StudentVersionObserver _observer;

        private void Start()
        {
            _bannerRoot.SetActive(false);

            _observer.OnStudentVersionOutdated
                .Subscribe(isOutdated => _bannerRoot.SetActive(isOutdated))
                .AddTo(this);
        }
    }
}
