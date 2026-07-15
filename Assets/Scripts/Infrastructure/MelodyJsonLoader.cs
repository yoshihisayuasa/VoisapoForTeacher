using Assets.Scripts.Domain.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// メロディJSON ローダー／セーバー（純粋I/O・曲数制限なし）
    /// </summary>
    public static class MelodyJsonLoader
    {
        // ── 公開API ──────────────────────────────────────────────────

        /// <summary>
        /// メロディエントリリストを読み込む。フォーマットの新旧はファイル名で判別する。
        /// 新フォーマットは fileName、旧フォーマットは legacyFileName に保存されている前提。
        /// 旧フォーマットしか無ければ新フォーマットへ移行して書き出す。
        /// </summary>
        public static List<SavedMelody> LoadFromJsonResource(string fileName, string legacyFileName)
        {
            string path = GetPersistentPath(fileName);
            Debug.Log($"Persistent Path: {path}");

            // 1. 新フォーマットが既にあればそのまま読む。
            if (System.IO.File.Exists(path))
            {
                return MelodyJsonConverter.ToSavedMelodies(System.IO.File.ReadAllText(path), "保存データ");
            }

            // 2. 旧フォーマットが残っていれば移行して新フォーマットで書き出す。
            string legacyPath = GetPersistentPath(legacyFileName);
            if (System.IO.File.Exists(legacyPath))
            {
                Debug.Log("旧フォーマットを検出。自動マイグレーションを実行します。");
                var legacy = LegacyMelodyMigration.Parse(System.IO.File.ReadAllText(legacyPath));
                var migrated = MergeLegacyWithSeed(legacy, fileName);
                System.IO.File.WriteAllText(path, SerializeNew(migrated));
                Debug.Log($"マイグレーション完了: {migrated.Count} 曲");
                return migrated;
            }

            // 3. どちらも無い初回起動はシード（Resources）から生成する。
            var seed = Resources.Load<TextAsset>(fileName);
            if (seed == null)
            {
                Debug.LogError($"シードJSONのロードに失敗: {fileName}");
                return new List<SavedMelody>();
            }
            System.IO.File.WriteAllText(path, seed.text);
            return MelodyJsonConverter.ToSavedMelodies(seed.text, "シードJSON");
        }

        /// <summary>
        /// 各メロディの Position のみを保存する。
        /// </summary>
        public static void SavePositions(string fileName, IReadOnlyList<SavedMelody> entries)
        {
            string path = GetPersistentPath(fileName);
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError($"保存ファイルが見つかりません: {path}");
                return;
            }
            try
            {
                var wrapper = JsonUtility.FromJson<MelodyListWrapper>(
                    System.IO.File.ReadAllText(path));

                // 名前で突き合わせて Position を更新
                foreach (var data in wrapper.Melodies)
                {
                    foreach (var entry in entries)
                    {
                        if (entry.Melody.Name == data.Name)
                        {
                            data.Position = entry.Position;
                            break;
                        }
                    }
                }

                System.IO.File.WriteAllText(path, JsonUtility.ToJson(wrapper, true));
                Debug.Log($"Position を保存しました: {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON保存エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 全メロディを保存する。
        /// </summary>
        public static void SaveAllMelodies(string fileName, IReadOnlyList<SavedMelody> entries)
        {
            try
            {
                string path = GetPersistentPath(fileName);
                System.IO.File.WriteAllText(path, SerializeNew(entries));
                Debug.Log($"メロディを保存しました: {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON保存エラー: {ex.Message}");
            }
        }

        // ── プライベートヘルパー ──────────────────────────────────────

        private static string GetPersistentPath(string fileName)
            => System.IO.Path.Combine(Application.persistentDataPath, fileName + ".json");

        private static string SerializeNew(IReadOnlyList<SavedMelody> entries)
        {
            var wrapper = new MelodyListWrapper { Melodies = new List<MelodyData>() };
            foreach (var entry in entries)
            {
                wrapper.Melodies.Add(MelodyJsonConverter.ToData(entry.Melody, entry.Position));
            }
            return JsonUtility.ToJson(wrapper, true);
        }

        /// <summary>
        /// 旧フォーマットを移行する。シードの組み込みメロディ（削除フラグ付き）を土台に、
        /// 旧データからは名前が重複しない自作メロディのみを追加してマージする。
        /// </summary>
        private static List<SavedMelody> MergeLegacyWithSeed(IReadOnlyList<SavedMelody> legacy, string fileName)
        {
            var merged = LoadSeedBuiltIns(fileName);

            var usedNames = new HashSet<string>();
            foreach (var entry in merged)
            {
                usedNames.Add(entry.Melody.Name);
            }

            foreach (var entry in legacy)
            {
                // メロディ名はアプリ全体で一意（SavePositions が名前で保存ファイルと突き合わせるため）。
                // 旧フォーマットは一意性を保証しないので、組み込みとの重複も旧データ内の重複もここで弾く。
                if (!usedNames.Add(entry.Melody.Name))
                {
                    Debug.LogError($"同名のメロディが既にあるため移行をスキップします: {entry.Melody.Name}");
                    continue;
                }
                merged.Add(entry);
            }

            for (int i = 0; i < merged.Count; i++)
            {
                merged[i].SetPosition(i);
            }
            return merged;
        }

        // シード（Resources）の組み込みメロディを読み込む。組み込みの削除フラグはシードJSONが持つ。
        private static List<SavedMelody> LoadSeedBuiltIns(string fileName)
        {
            var textAsset = Resources.Load<TextAsset>(fileName);
            if (textAsset == null)
            {
                Debug.LogError($"シードJSONのロードに失敗: {fileName}");
                return new List<SavedMelody>();
            }
            return MelodyJsonConverter.ToSavedMelodies(textAsset.text, "シードJSON");
        }
    }
}
