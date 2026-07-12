using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;
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
                return ParseNewText(System.IO.File.ReadAllText(path));
            }

            // 2. 旧フォーマットが残っていれば移行して新フォーマットで書き出す。
            string legacyPath = GetPersistentPath(legacyFileName);
            if (System.IO.File.Exists(legacyPath))
            {
                Debug.Log("旧フォーマットを検出。自動マイグレーションを実行します。");
                var legacy = JsonUtility.FromJson<LegacyScaleDataWrapper>(
                    System.IO.File.ReadAllText(legacyPath));
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
            return ParseNewText(seed.text);
        }

        // 新フォーマットのJSON文字列をエントリリストへ復元する。
        private static List<SavedMelody> ParseNewText(string jsonText)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<MelodyListWrapper>(jsonText);
                if (wrapper == null || wrapper.Melodies == null || wrapper.Melodies.Count == 0)
                {
                    Debug.LogError("JSONデータが不正です");
                    return new List<SavedMelody>();
                }
                return ParseNew(wrapper);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSONパースエラー: {ex.Message}");
                return new List<SavedMelody>();
            }
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

        /// <summary>
        /// 単一メロディを JSON 文字列へシリアライズする（ネットワーク送信用）。
        /// </summary>
        public static string SerializeMelody(Melody melody)
        {
            return JsonUtility.ToJson(MelodyJsonConverter.ToData(melody, 0));
        }

        /// <summary>
        /// JSON 文字列から単一メロディを復元する（ネットワーク受信用）。失敗時は null。
        /// </summary>
        public static Melody DeserializeMelody(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var data = JsonUtility.FromJson<MelodyData>(json);
                if (data == null || string.IsNullOrEmpty(data.Name)) return null;
                return MelodyJsonConverter.ToMelody(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"メロディの復元に失敗: {ex.Message}");
                return null;
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

        private static List<SavedMelody> ParseNew(MelodyListWrapper wrapper)
        {
            var entries = new List<SavedMelody>();
            foreach (var data in wrapper.Melodies)
            {
                if (string.IsNullOrEmpty(data.Name)) continue;
                entries.Add(new SavedMelody(MelodyJsonConverter.ToMelody(data), data.Position));
            }
            return entries;
        }

        /// <summary>
        /// 旧フォーマットを移行する。シードの組み込みメロディ（削除フラグ付き）を土台に、
        /// 旧データからは組み込みと名前が重複しない自作メロディのみを追加してマージする。
        /// </summary>
        private static List<SavedMelody> MergeLegacyWithSeed(LegacyScaleDataWrapper legacy, string fileName)
        {
            var merged = LoadSeedBuiltIns(fileName);

            var builtInNames = new HashSet<string>();
            foreach (var entry in merged)
            {
                builtInNames.Add(entry.Melody.Name);
            }

            foreach (var entry in ParseLegacy(legacy))
            {
                if (builtInNames.Contains(entry.Melody.Name)) continue;
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
            var wrapper = JsonUtility.FromJson<MelodyListWrapper>(textAsset.text);
            if (wrapper == null || wrapper.Melodies == null)
            {
                return new List<SavedMelody>();
            }
            return ParseNew(wrapper);
        }

        // 旧フォーマットの和音は常に3拍固定（構成音数 Chord.Length と同値なのは偶然）。
        private const int LegacyChordBeats = 3;

        /// <summary>
        /// 旧フォーマットは種別を持たないため、表示名から再生種別を解決する。
        /// 名前と種別の対応は旧フォーマット移行だけが知る（新フォーマットは Kind をデータに持つ）。
        /// </summary>
        private static MelodyKind ResolveLegacyKind(string name) => name switch
        {
            "Single"           => MelodyKind.Single,
            "Major& Metronome" => MelodyKind.ChordWithMetronome,
            "Major Code"       => MelodyKind.Chord,
            _                  => MelodyKind.Standard,
        };

        private static List<SavedMelody> ParseLegacy(LegacyScaleDataWrapper legacy)
        {
            var type = typeof(LegacyScaleDataWrapper);
            var entries = new List<SavedMelody>();

            // legacyFileName に旧フォーマット以外（例: 開発端末に残った新フォーマット）が
            // 置かれていた場合は ScaleName が null。自作なしとして扱い、シードのみで再構成させる。
            if (legacy == null || legacy.ScaleName == null)
            {
                return entries;
            }

            for (int i = 0; i < legacy.ScaleName.Count; i++)
            {
                string name = legacy.ScaleName[i].Replace("～", "~").Replace("*", "");
                if (string.IsNullOrEmpty(name)) continue;

                int position = legacy.ScalePos[i];

                var noteArr = type.GetField($"ScaleNote{i}")?.GetValue(legacy) as int[];
                var beatArr = type.GetField($"ScaleBeat{i}")?.GetValue(legacy) as int[];
                int len = noteArr?.Length ?? 0;

                var chordIntervals = new List<Interval>();
                var notes = new List<Note>();

                for (int j = 0; j < len; j++)
                {
                    int interval = noteArr[j];
                    int beat = (beatArr != null && j < beatArr.Length) ? beatArr[j] : 0;

                    if (j < Chord.Length)
                    {
                        chordIntervals.Add(new Interval(interval));
                    }
                    else
                    {
                        if (beat == 0) break;
                        notes.Add(new Note(interval, beat));
                    }
                }

                var chord = new Chord(chordIntervals, LegacyChordBeats);
                entries.Add(new SavedMelody(
                    new Melody(name, ResolveLegacyKind(name), chord, notes, isProtected: false, isUserCreated: true), position));
            }
            return entries;
        }

        // ── 旧フォーマット用（マイグレーションのみ・削除不可） ────────

        [Serializable]
        private class LegacyScaleDataWrapper
        {
            public List<string> ScaleName;
            public List<int> ScalePos;
            public int ScaleNum;
            public int[] ScaleBeat0; public int[] ScaleBeat1; public int[] ScaleBeat2;
            public int[] ScaleBeat3; public int[] ScaleBeat4; public int[] ScaleBeat5;
            public int[] ScaleBeat6; public int[] ScaleBeat7; public int[] ScaleBeat8;
            public int[] ScaleBeat9; public int[] ScaleBeat10; public int[] ScaleBeat11;
            public int[] ScaleBeat12; public int[] ScaleBeat13; public int[] ScaleBeat14;
            public int[] ScaleBeat15; public int[] ScaleBeat16; public int[] ScaleBeat17;
            public int[] ScaleBeat18; public int[] ScaleBeat19; public int[] ScaleBeat20;
            public int[] ScaleBeat21; public int[] ScaleBeat22; public int[] ScaleBeat23;
            public int[] ScaleBeat24; public int[] ScaleBeat25; public int[] ScaleBeat26;
            public int[] ScaleNote0; public int[] ScaleNote1; public int[] ScaleNote2;
            public int[] ScaleNote3; public int[] ScaleNote4; public int[] ScaleNote5;
            public int[] ScaleNote6; public int[] ScaleNote7; public int[] ScaleNote8;
            public int[] ScaleNote9; public int[] ScaleNote10; public int[] ScaleNote11;
            public int[] ScaleNote12; public int[] ScaleNote13; public int[] ScaleNote14;
            public int[] ScaleNote15; public int[] ScaleNote16; public int[] ScaleNote17;
            public int[] ScaleNote18; public int[] ScaleNote19; public int[] ScaleNote20;
            public int[] ScaleNote21; public int[] ScaleNote22; public int[] ScaleNote23;
            public int[] ScaleNote24; public int[] ScaleNote25; public int[] ScaleNote26;
        }
    }
}
