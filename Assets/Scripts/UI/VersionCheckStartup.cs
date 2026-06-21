using AsseScripts.Domain;
using AsseScripts.Infrastructure;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace Assets.Scripts.UI
{
    public sealed class VersionCheckStartup : MonoBehaviour
    {
        private const string ModalResourcePath =
            "Prefab/UnityScreenNavigator/Modal/pfb_ui_modal_version_update";

        [SerializeField] private string _versionJsonUrl;

        private void Start()
        {
            CheckVersionAsync().Forget();
        }

        private async UniTaskVoid CheckVersionAsync()
        {
            var fetcher = new JsonVersionFetcher(_versionJsonUrl);
            var result = await fetcher.FetchLatestAsync();

            if (result.HasUpdate)
                ShowUpdatePopup(result.LatestVersion, result.DownloadUrl);
        }

        private void ShowUpdatePopup(AppVersion latestVersion, string downloadUrl)
        {
            ModalContainer.Find("ModalContainer").Push(ModalResourcePath, true, onLoad: x =>
            {
                var ui = x.modal.GetComponent<VersionUpdateUI>();
                ui.Setup(latestVersion, downloadUrl);
            });
        }
    }
}
