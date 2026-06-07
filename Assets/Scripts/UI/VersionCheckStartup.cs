using AsseScripts.Domain;
using AsseScripts.Infrastructure;
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
            IVersionFetcher fetcher = new JsonVersionFetcher(_versionJsonUrl);
            var result = await fetcher.FetchLatestAsync();

            if (result.HasUpdate)
                ShowUpdatePopup(result.LatestVersion, result.DownloadUrl);
        }

        private static void ShowUpdatePopup(AppVersion latestVersion, string downloadUrl)
        {
            SimpleModalWindow.Create(ignorable: true)
                .SetHeader("アップデートがあります")
                .SetBody($"最新版 {latestVersion} が公開されています。")
                .AddButton("後で", null, ModalButtonType.Danger)
                .AddButton("ダウンロードページへ", () => Application.OpenURL(downloadUrl), ModalButtonType.Success)
                .Show();
        }
    }
}
