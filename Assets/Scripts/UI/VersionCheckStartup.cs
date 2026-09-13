using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Infrastructure;
using Assets.Scripts.UI.Modal;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public sealed class VersionCheckStartup : MonoBehaviour
    {
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
            var body = new LocalizedMessage(
                japanese: $"バージョン {latestVersion} にアップデートしてください。",
                english: $"Please update to version {latestVersion}.");

            ConfirmModalUI.Show(
                body.ForCurrentLanguage(),
                onConfirm: () => Application.OpenURL(downloadUrl),
                onCancel: () => { });
        }
    }
}
