using R3;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class VersionBannerUI : MonoBehaviour
    {
        [SerializeField] private GameObject _bannerRoot;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField, TextArea] private string _message;
        [SerializeField] private VersionObserver _observer;

        private void Start()
        {
            _messageText.text = _message;

            _observer.OnPeerVersionOutdated
                .Subscribe(isOutdated => _bannerRoot.SetActive(isOutdated))
                .AddTo(this);
        }
    }
}
