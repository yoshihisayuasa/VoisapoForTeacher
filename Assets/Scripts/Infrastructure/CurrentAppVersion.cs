using Assets.Scripts.Domain.ValueObjects;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// 実行中のアプリバージョン。通常は PlayerSettings の値（Application.version）を返す。
    /// エディタ実行時のみ Tools > Voisapo > アプリバージョン から一時的に差し替えられ、
    /// 更新通知や相手バージョン警告をテストできる。差し替えはビルドには影響しない。
    /// </summary>
    public static class CurrentAppVersion
    {
        public static AppVersion Value
        {
            get
            {
#if UNITY_EDITOR
                if (AppVersion.TryCreate(Override, out var overridden))
                {
                    return overridden;
                }
#endif
                return new AppVersion(Application.version);
            }
        }

#if UNITY_EDITOR
        private const string OverrideKey = "Voisapo.AppVersionOverride";

        /// <summary>テスト用に差し替えるバージョン。空文字なら実際のバージョンを使う。</summary>
        public static string Override
        {
            get => UnityEditor.EditorPrefs.GetString(OverrideKey, string.Empty);
            set => UnityEditor.EditorPrefs.SetString(OverrideKey, value);
        }
#endif
    }
}
