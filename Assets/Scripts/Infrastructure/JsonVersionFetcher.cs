using Assets.Scripts.Domain.ValueObjects;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Infrastructure
{
    public sealed class JsonVersionFetcher
    {
        private readonly string _jsonUrl;

        public JsonVersionFetcher(string jsonUrl)
        {
            _jsonUrl = jsonUrl;
        }

        public async UniTask<VersionCheckResult> FetchLatestAsync()
        {
            using var request = UnityWebRequest.Get(_jsonUrl);
            try
            {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                    return VersionCheckResult.FetchFailed();

                var data = JsonUtility.FromJson<VersionJson>(request.downloadHandler.text);
                bool isMac = Application.platform == RuntimePlatform.OSXPlayer;
                var storeVersion = new AppVersion(isMac ? data.versionMac : data.versionWindows);
                var currentVersion = new AppVersion(Application.version);

                if (storeVersion.IsNewerThan(currentVersion))
                {
                    string downloadUrl = isMac ? data.downloadUrlMac : data.downloadUrlWindows;
                    return VersionCheckResult.UpdateAvailable(storeVersion, downloadUrl);
                }

                return VersionCheckResult.UpToDate();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VersionCheck] JSON取得失敗: {ex.Message}");
                return VersionCheckResult.FetchFailed();
            }
        }

        [Serializable]
        private class VersionJson
        {
            public string versionMac;
            public string versionWindows;
            public string downloadUrlMac;
          public string downloadUrlWindows;
        }
    }
}
