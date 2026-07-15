using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// メロディJSONのシリアライズ用DTOと、DTO⇔Melody の変換。
    /// 保存ファイル・テンプレート・ネットワーク送受信はすべて同じフォーマットを使うため、
    /// DTO定義と変換ロジックはここ1箇所だけが持つ。
    /// </summary>
    internal static class MelodyJsonConverter
    {
        /// <summary>
        /// メロディリストのJSON文字列を復元する。読めない1件はスキップして残りを返す（部分復旧）。
        /// source はエラーログでどのJSONの話かを示すための呼び出し元の名前。
        /// </summary>
        internal static List<SavedMelody> ToSavedMelodies(string jsonText, string source)
        {
            var entries = new List<SavedMelody>();

            MelodyListWrapper wrapper;
            try
            {
                wrapper = JsonUtility.FromJson<MelodyListWrapper>(jsonText);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{source}のJSONパースエラー: {ex.Message}");
                return entries;
            }

            if (wrapper == null || wrapper.Melodies == null || wrapper.Melodies.Count == 0)
            {
                Debug.LogError($"{source}のJSONデータが不正です");
                return entries;
            }

            foreach (var data in wrapper.Melodies)
            {
                try
                {
                    if (string.IsNullOrEmpty(data.Name)) continue;
                    entries.Add(new SavedMelody(ToMelody(data), data.Position));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"{source}のメロディ「{data?.Name}」を読み込めないためスキップします: {ex.Message}");
                }
            }
            return entries;
        }

        internal static Melody ToMelody(MelodyData data)
        {
            var intervals = new List<Interval>();
            foreach (var v in data.Chord.Intervals)
            {
                intervals.Add(new Interval(v));
            }
            var chord = new Chord(intervals, data.Chord.Beats);

            var notes = new List<Note>();
            if (data.Notes != null)
            {
                foreach (var n in data.Notes)
                {
                    notes.Add(new Note(n.Interval, n.Beats));
                }
            }
            return new Melody(data.Name, ParseKind(data.Kind), chord, notes, data.IsProtected, data.IsUserCreated);
        }

        // 種別はJSONに文字列で持つ（intだとenumの並び替えでデータが壊れるため）。
        // 欠損・不明な値は Standard（通常再生）として扱う。
        private static MelodyKind ParseKind(string kindName) =>
            Enum.TryParse(kindName, out MelodyKind kind) ? kind : MelodyKind.Standard;

        internal static MelodyData ToData(Melody melody, int position)
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
                Position = position,
                Kind = melody.Kind.ToString(),
                IsProtected = melody.IsProtected,
                IsUserCreated = melody.IsUserCreated,
                Chord = chordData,
                Notes = new List<NoteData>()
            };
            foreach (var note in melody.Notes)
            {
                data.Notes.Add(new NoteData { Interval = note.Interval.Value, Beats = note.Beats });
            }
            return data;
        }
    }

    [Serializable]
    internal class MelodyListWrapper
    {
        public List<MelodyData> Melodies;
    }

    [Serializable]
    internal class MelodyData
    {
        public string Name;
        public int Position;
        public string Kind;
        public bool IsProtected;
        public bool IsUserCreated;
        public ChordData Chord;
        public List<NoteData> Notes;
    }

    [Serializable]
    internal class ChordData
    {
        public List<int> Intervals;
        public int Beats;
    }

    [Serializable]
    internal class NoteData
    {
        public int Interval;
        public int Beats;
    }
}
