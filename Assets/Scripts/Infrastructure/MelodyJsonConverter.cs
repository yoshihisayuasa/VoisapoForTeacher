using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// メロディJSONのシリアライズ用DTOと、DTO⇔Melody の変換。
    /// 保存ファイル・テンプレート・ネットワーク送受信はすべて同じフォーマットを使うため、
    /// DTO定義と変換ロジックはここ1箇所だけが持つ。
    /// </summary>
    internal static class MelodyJsonConverter
    {
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
            return new Melody(data.Name, chord, notes);
        }

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
