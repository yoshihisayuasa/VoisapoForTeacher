using System;
using System.Collections.Generic;
using UnityEngine;
using DomainChord = AsseScripts.Domain.Chord;
using DomainMelody = AsseScripts.Domain.Melody;
using DomainNote = AsseScripts.Domain.Note;

namespace AsseScripts.Infrastructure
{
    /// <summary>
    /// メロディJSON ローダー／セーバー（純粋I/O・曲数制限なし）
    /// </summary>
    public static class MelodyJsonLoader
    {
        // ── 公開API ──────────────────────────────────────────────────

        /// <summary>
        /// JSONファイルからメロディリストを読み込む。
        /// 旧フォーマットを検出した場合は自動マイグレーションを行う。
        /// </summary>
        public static List<DomainMelody> LoadFromJsonResource(string fileName)
        {
            string path = GetPersistentPath(fileName);
            string jsonText;

            Debug.Log($"Persistent Path: {path}");

            if (System.IO.File.Exists(path))
            {
                jsonText = System.IO.File.ReadAllText(path);
            }
            else
            {
                var textAsset = Resources.Load<TextAsset>(fileName);
                if (textAsset == null)
                {
                    Debug.LogError($"JSONファイルのロードに失敗: {fileName}");
                    return new List<DomainMelody>();
                }
                jsonText = textAsset.text;
                System.IO.File.WriteAllText(path, jsonText);
            }

            try
            {
                // 旧フォーマット検出（ScaleNum フィールドが存在する）
                var legacy = JsonUtility.FromJson<LegacyScaleDataWrapper>(jsonText);
                if (legacy != null && legacy.ScaleNum > 0 && legacy.ScaleName != null)
                {
                    Debug.Log("旧フォーマットを検出。自動マイグレーションを実行します。");
                    var migrated = ParseLegacy(legacy);
                    System.IO.File.WriteAllText(path, SerializeNew(migrated));
                    Debug.Log($"マイグレーション完了: {migrated.Count} 曲");
                    return migrated;
                }

                // 新フォーマット
                var wrapper = JsonUtility.FromJson<MelodyListWrapper>(jsonText);
                if (wrapper.Melodies == null || wrapper.Melodies.Count == 0)
                {
                    Debug.LogError("JSONデータが不正です");
                    return new List<DomainMelody>();
                }
                return ParseNew(wrapper);
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSONパースエラー: {ex.Message}");
                return new List<DomainMelody>();
            }
        }

        /// <summary>
        /// 各メロディの Position のみを保存する。
        /// </summary>
        public static void SavePositions(string fileName, IReadOnlyList<DomainMelody> melodies)
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
                    foreach (var melody in melodies)
                    {
                        if (melody.Name == data.Name)
                        {
                            data.Position = melody.Position;
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
        public static void SaveAllMelodies(string fileName, IReadOnlyList<DomainMelody> melodies)
        {
            try
            {
                string path = GetPersistentPath(fileName);
                System.IO.File.WriteAllText(path, SerializeNew(melodies));
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

        private static string SerializeNew(IReadOnlyList<DomainMelody> melodies)
        {
            var wrapper = new MelodyListWrapper { Melodies = new List<MelodyData>() };
            foreach (var melody in melodies)
            {
                var chordData = new ChordData
                {
                    Intervals = new List<int>(),
                    Beats = melody.Chord.Beats
                };
                foreach (var interval in melody.Chord.Intervals)
                {
                    chordData.Intervals.Add(interval.Value);
                }

                var data = new MelodyData
                {
                    Name = melody.Name,
                    Position = melody.Position,
                    Chord = chordData,
                    Notes = new List<NoteData>()
                };
                foreach (var note in melody.Notes)
                {
                    data.Notes.Add(new NoteData { Interval = note.Interval.Value, Beats = note.Beats });
                }
                wrapper.Melodies.Add(data);
            }
            return JsonUtility.ToJson(wrapper, true);
        }

        private static DomainChord ParseChordData(ChordData chordData)
        {
            var intervals = new List<Assets.Scripts.Domain.ValueObjects.Interval>();
            foreach (var v in chordData.Intervals)
            {
                intervals.Add(new Assets.Scripts.Domain.ValueObjects.Interval(v));
            }
            return new DomainChord(intervals, chordData.Beats);
        }

        private static List<DomainMelody> ParseNew(MelodyListWrapper wrapper)
        {
            var melodies = new List<DomainMelody>();
            foreach (var data in wrapper.Melodies)
            {
                if (string.IsNullOrEmpty(data.Name))
                {
                    continue;
                }

                var chord = ParseChordData(data.Chord);

                var notes = new List<DomainNote>();
                if (data.Notes != null)
                {
                    foreach (var n in data.Notes)
                    {
                        notes.Add(new DomainNote(n.Interval, n.Beats));
                    }
                }
                melodies.Add(new DomainMelody(data.Name, chord, notes, data.Position));
            }
            return melodies;
        }

        private static List<DomainMelody> ParseLegacy(LegacyScaleDataWrapper legacy)
        {
            var type = typeof(LegacyScaleDataWrapper);
            var melodies = new List<DomainMelody>();

            for (int i = 0; i < legacy.ScaleName.Count; i++)
            {
                string name = legacy.ScaleName[i].Replace("～", "~").Replace("*", "");
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                int position = legacy.ScalePos[i] ;

                var noteArr = type.GetField($"ScaleNote{i}")?.GetValue(legacy) as int[];
                var beatArr = type.GetField($"ScaleBeat{i}")?.GetValue(legacy) as int[];
                int len = noteArr.Length;

                // 旧フォーマット: 先頭3音 = 和音、残り = メロディ
                int legacyCordLength = DomainChord.Length;
                var chordIntervals = new List<Assets.Scripts.Domain.ValueObjects.Interval>();
                var notes = new List<DomainNote>();

                for (int j = 0; j < len; j++)
                {
                    int interval = (noteArr != null && j < noteArr.Length) ? noteArr[j] : 0;
                    int beat = (beatArr != null && j < beatArr.Length) ? beatArr[j] : 0;

                    if (j < legacyCordLength)
                    {
                        chordIntervals.Add(new Assets.Scripts.Domain.ValueObjects.Interval(interval));
                    }
                    else
                    {
                        if (beat == 0)
                        {
                            break;
                        }
                        notes.Add(new DomainNote(interval, beat));
                    }
                }

                // 和音が3音未満の場合はルート(0)で埋める
                while (chordIntervals.Count < legacyCordLength)
                {
                    chordIntervals.Add(new Assets.Scripts.Domain.ValueObjects.Interval(0));
                }

                var chord = new DomainChord(chordIntervals, legacyCordLength);
                melodies.Add(new DomainMelody(name, chord, notes, position));
            }
            return melodies;
        }

        // ── 新フォーマット用シリアライズクラス ────────────────────────

        [Serializable]
        private class MelodyListWrapper
        {
            public List<MelodyData> Melodies;
        }

        [Serializable]
        private class MelodyData
        {
            public string Name;
            public int Position;
            public ChordData Chord;
            public List<NoteData> Notes;
        }

        [Serializable]
        private class ChordData
        {
            public List<int> Intervals;
            public int Beats;
        }

        [Serializable]
        private class NoteData
        {
            public int Interval;
            public int Beats;
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