using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Infrastructure;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    /// <summary>
    /// PianoSoundSetにEntriesを自動登録するエディタツール。
    /// 選択したPianoSoundSetアセットに、指定フォルダの音源を一括セットする。
    /// </summary>
    public static class PianoSoundSetBuilder
    {
        /// <summary>
        /// enum名（例: C2Sharp）をファイル名（例: C#2）に変換する。
        /// "Sharp"サフィックスを"#"に置換し、数字の前に挿入する。
        /// </summary>
        private static string EnumNameToFileName(string enumName)
        {
            if (!enumName.EndsWith("Sharp")) return enumName;

            var withoutSharp = enumName[..^5];
            int digitStart = withoutSharp.IndexOfAny("0123456789".ToCharArray());
            if (digitStart < 0) return enumName;

            return withoutSharp[..digitStart] + "#" + withoutSharp[digitStart..];
        }

        [MenuItem("Voisapo/Build PianoSoundSet from folder")]
        public static void BuildFromFolder()
        {
            var soundSet = Selection.activeObject as PianoSoundSet;
            if (soundSet == null)
            {
                EditorUtility.DisplayDialog("エラー", "ProjectウィンドウでPianoSoundSetアセットを選択してから実行してください。", "OK");
                return;
            }

            var folder = EditorUtility.OpenFolderPanel("音源フォルダを選択", "Assets/Audio", "");
            if (string.IsNullOrEmpty(folder)) return;

            // 絶対パスをAssets相対パスに変換
            var dataPath = Application.dataPath;
            if (!folder.StartsWith(dataPath))
            {
                EditorUtility.DisplayDialog("エラー", "Assetsフォルダ内のフォルダを選択してください。", "OK");
                return;
            }
            var relativeFolderPath = "Assets" + folder[dataPath.Length..];

            // Entriesをリフレクションで取得してクリア・再構築
            var so = new SerializedObject(soundSet);
            var entriesProp = so.FindProperty("_entries");
            entriesProp.ClearArray();

            int index = 0;
            int missing = 0;
            foreach (PianoNoteEnum note in System.Enum.GetValues(typeof(PianoNoteEnum)))
            {
                if (note == PianoNoteEnum.None) continue;

                var fileName = EnumNameToFileName(note.ToString());
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{relativeFolderPath}/{fileName}{".ogg"}")
                        ?? LoadClip(relativeFolderPath, fileName, ".oga");

                if (clip == null)
                {
                    Debug.LogWarning($"[PianoSoundSetBuilder] {fileName} が見つかりません（フォルダ: {relativeFolderPath}）");
                    missing++;
                }

                entriesProp.InsertArrayElementAtIndex(index);
                var element = entriesProp.GetArrayElementAtIndex(index);
                var noteProp = element.FindPropertyRelative("Note");
                noteProp.enumValueIndex = System.Array.IndexOf(noteProp.enumNames, note.ToString());
                element.FindPropertyRelative("Clip").objectReferenceValue = clip;
                index++;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(soundSet);
            AssetDatabase.SaveAssets();

            var msg = missing == 0
                ? $"{index}個のエントリを登録しました。"
                : $"{index}個中{missing}個の音源が見つかりませんでした。";
            EditorUtility.DisplayDialog("完了", msg, "OK");
        }

        private static AudioClip LoadClip(string relativeFolderPath, string fileName, string v)
        {
            throw new NotImplementedException();
        }
    }
}
