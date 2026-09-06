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
                {
                    Debug.LogWarning($"[VersionCheck] 取得失敗: {_jsonUrl} ({request.error})");
                    return VersionCheckResult.FetchFailed();
                }

                var data = JsonUtility.FromJson<VersionJson>(request.downloadHandler.text);

                // 該当ストアのバージョンが JSON に無い（未設定）ときは判定できない。
                if (!AppVersion.TryCreate(data.StoreVersion(), out var storeVersion))
                {
                    Debug.LogWarning($"[VersionCheck] JSON のバージョンが未設定または不正: '{data.StoreVersion()}'");
                    return VersionCheckResult.FetchFailed();
                }

                var currentVersion = CurrentAppVersion.Value;
                Debug.Log($"[VersionCheck] ストア {storeVersion} / 自分 {currentVersion}");

                if (storeVersion.IsNewerThan(currentVersion))
                {
                    return VersionCheckResult.UpdateAvailable(storeVersion, data.DownloadUrl());
                }

                return VersionCheckResult.UpToDate();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VersionCheck] JSON取得失敗: {ex.Message}");
                return VersionCheckResult.FetchFailed();
            }
        }

        /// <summary>
        /// 先生用（Mac / Windows）と生徒用（iPhone / Android）はストア上で別プロダクトのため、
        /// それぞれのバージョンと配布URLを持つ。student～ が生徒用。
        /// 起動中のビルドと実行OSに対応する値だけを使う。
        /// </summary>
        [Serializable]
        private class VersionJson
        {
            public string versionMac;
            public string versionWindows;
            public string downloadUrlMac;
            public string downloadUrlWindows;
            public string studentVersioniPhone;
            public string studentVersionAndroid;
            public string studentDownloadUrliPhone;
            public string studentDownloadUrlAndroid;

            private static bool IsMac => Application.platform == RuntimePlatform.OSXPlayer;

            private static bool IsIPhone => Application.platform == RuntimePlatform.IPhonePlayer;

            public string StoreVersion()
            {
                if (AppMode.IsTeacher)
                    return IsMac ? versionMac : versionWindows;
                return IsIPhone ? studentVersioniPhone : studentVersionAndroid;
            }

            public string DownloadUrl()
            {
                if (AppMode.IsTeacher)
                    return IsMac ? downloadUrlMac : downloadUrlWindows;
                return IsIPhone ? studentDownloadUrliPhone : studentDownloadUrlAndroid;
            }
        }
    }
}
