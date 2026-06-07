using AsseScripts.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace Assets.Scripts.UI
{
    public sealed class VersionUpdateUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private Button _downloadButton;
        [SerializeField] private Button _dismissButton;

        private string _downloadUrl;

        public void Setup(AppVersion version, string downloadUrl)
        {
            _downloadUrl = downloadUrl;
            _bodyText.text = $"Version {version} is available";
            _downloadButton.onClick.AddListener(() => Application.OpenURL(_downloadUrl));
            _dismissButton.onClick.AddListener(() => ModalContainer.Of(transform).Pop(true));
        }
    }
}
