using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// 旧フォーマット（曲ごとに ScaleNote0..26 / ScaleBeat0..26 のフィールドを持つ形式）を
    /// 現行のメロディへ移行する。旧フォーマットの約束事はこのクラスだけが知る。
    /// </summary>
    internal static class LegacyMelodyMigration
    {
        // 旧フォーマットの和音は常に3拍固定（構成音数 Chord.Length と同値なのは偶然）。
        private const int LegacyChordBeats = 3;

        /// <summary>
        /// 旧フォーマットのJSON文字列をメロディへ復元する。旧フォーマットとして読めなければ空を返す。
        /// </summary>
        internal static List<SavedMelody> Parse(string jsonText)
        {
            var entries = new List<SavedMelody>();

            LegacyScaleDataWrapper legacy;
            try
            {
                legacy = JsonUtility.FromJson<LegacyScaleDataWrapper>(jsonText);
            }
            catch (Exception ex)
            {
                Debug.LogError($"旧フォーマットのJSONパースエラー: {ex.Message}");
                return entries;
            }

            // 旧フォーマット以外（例: 開発端末に残った新フォーマット）が置かれていた場合は ScaleName が null。
            // 自作なしとして扱い、シードのみで再構成させる。
            if (legacy == null || legacy.ScaleName == null)
            {
                return entries;
            }

            var type = typeof(LegacyScaleDataWrapper);
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

                // 旧フォーマットは和音とメロディを1本の配列に並べて持つ。先頭 Chord.Length 音が和音。
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
                    new Melody(name, MelodyKinds.FromDisplayName(name), chord, notes, isProtected: false, isPremiumOnly: true),
                    position));
            }
            return entries;
        }

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
