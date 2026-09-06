using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Infrastructure;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    /// <summary>
    /// エディタ実行時のアプリバージョンを一時的に差し替える開発用ウィンドウ。
    /// 古いバージョンにしておくと、更新通知（VersionCheckStartup）や
    /// 相手バージョン警告（VersionObserver）の動作を確認できる。
    /// </summary>
    public sealed class AppVersionOverrideWindow : EditorWindow
    {
        private string _input;

        [MenuItem("Tools/Voisapo/アプリバージョン...")]
        private static void Open()
        {
            var window = GetWindow<AppVersionOverrideWindow>(utility: true, title: "アプリバージョン（テスト用）");
            window.minSize = new Vector2(400f, 160f);
        }

        // 差し替えたまま忘れないよう、再生開始時に知らせる。
        [InitializeOnLoadMethod]
        private static void WarnOnPlayMode()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode) return;
                if (string.IsNullOrEmpty(CurrentAppVersion.Override)) return;

                Debug.LogWarning($"[テスト] アプリバージョンを {CurrentAppVersion.Override} に差し替え中（実際は {Application.version}）");
            };
        }

        private void OnEnable()
        {
            _input = string.IsNullOrEmpty(CurrentAppVersion.Override)
                ? Application.version
                : CurrentAppVersion.Override;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("実際のバージョン", Application.version);
            _input = EditorGUILayout.TextField("テスト用バージョン", _input);

            bool isValid = AppVersion.TryCreate(_input, out _);
            if (!isValid)
            {
                EditorGUILayout.HelpBox($"無効なバージョン形式: {_input}", MessageType.Error);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!isValid))
                {
                    if (GUILayout.Button("適用"))
                    {
                        CurrentAppVersion.Override = _input;
                        GUI.FocusControl(null);
                    }
                }

                if (GUILayout.Button("実際のバージョンに戻す"))
                {
                    CurrentAppVersion.Override = string.Empty;
                    _input = Application.version;
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.Space();

            if (string.IsNullOrEmpty(CurrentAppVersion.Override))
            {
                EditorGUILayout.HelpBox("実際のバージョンで動作中。", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"{CurrentAppVersion.Override} に差し替え中。エディタ実行時のみ有効で、ビルドには影響しない。",
                    MessageType.Warning);
            }
        }
    }
}
