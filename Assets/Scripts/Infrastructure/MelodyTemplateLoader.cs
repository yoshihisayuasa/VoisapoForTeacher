using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    public static class MelodyTemplateLoader
    {
        public static IReadOnlyList<Melody> LoadFromResource(string fileName)
        {
            var asset = Resources.Load<TextAsset>(fileName);
            if (asset == null)
            {
                Debug.LogError($"テンプレートJSONのロードに失敗: {fileName}");
                return new List<Melody>();
            }

            try
            {
                var wrapper = JsonUtility.FromJson<TemplateListWrapper>(asset.text);
                return Parse(wrapper);
            }
            catch (Exception ex)
            {
                Debug.LogError($"テンプレートJSONパースエラー: {ex.Message}");
                return new List<Melody>();
            }
        }

        private static List<Melody> Parse(TemplateListWrapper wrapper)
        {
            var result = new List<Melody>();
            if (wrapper.Melodies == null)
            {
                return result;
            }
            foreach (var t in wrapper.Melodies)
            {
                if (string.IsNullOrEmpty(t.Name)) continue;

                var intervals = new List<Interval>();
                foreach (var v in t.Chord.Intervals)
                { 
                    intervals.Add(new Interval(v));
                }
                var chord = new Chord(intervals, t.Chord.Beats);

                var notes = new List<Note>();
                if (t.Notes != null)
                { 
                    foreach (var n in t.Notes)
                    { 
                        notes.Add(new Note(n.Interval, n.Beats));
                    }
                }
                result.Add(new Melody(t.Name, chord, notes));
            }
            return result;
        }

        [Serializable]
        private class TemplateListWrapper { public List<TemplateData> Melodies; }

        [Serializable]
        private class TemplateData
        {
            public string Name;
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
    }
}
