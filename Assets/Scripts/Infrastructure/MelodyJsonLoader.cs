using System;
using System.Collections.Generic;
using UnityEngine;
using DomainMelody = Scripts.Domain.Melody;
using DomainNote = Scripts.Domain.Note;

namespace Scripts.Infrastructure
{
    /// <summary>
    /// JSON���[�_�[
    /// </summary>
    public static class MelodyJsonLoader
    {

        private const int _noteNum = 28;

        public static List<DomainMelody> LoadFromJsonResource(string fileName)
        {
            var textAsset = Resources.Load<TextAsset>(fileName);
            if (textAsset == null)
            {
                Debug.LogError($"JSON�t�@�C���̃��[�h�Ɏ��s: {fileName}");
                return new List<DomainMelody>();
            }

            try
            {
                // JSON���p�[�X
                var json = JsonUtility.FromJson<ScaleDataWrapper>(textAsset.text);

                if (json == null || json.ScaleName == null || json.ScaleNum <= 0)
                {
                    Debug.LogError("JSON�f�[�^���s���ł�");
                    return new List<DomainMelody>();
                }

                var melodies = new List<DomainMelody>();
                int scaleCount = json.ScaleNum;

                for (int i = 0; i < json.ScaleName.Count; i++)
                {
                    string name = json.ScaleName[i];
                    if (string.IsNullOrEmpty(name)) continue;

                    var notes = new List<DomainNote>();
                    for (int j = 0; j < _noteNum; j++)
                    {
                        // "ScaleNote{j}"��"ScaleBeat{j}"�̔z�񂩂�l���擾
                        int interval = GetValueFromArray(json, $"ScaleNote{i}", j);
                        int beat = GetValueFromArray(json, $"ScaleBeat{i}", j);

                        notes.Add(new DomainNote(interval, beat));
                    }
                    melodies.Add(new DomainMelody(name, notes));
                }

                return melodies;
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON�p�[�X�G���[: {ex.Message}");
                return new List<DomainMelody>();
            }
        }

        private static int GetValueFromArray(ScaleDataWrapper data, string propName, int index)
        {
            var type = typeof(ScaleDataWrapper);
            var prop = type.GetField(propName);
            if (prop == null) return 0;
            var arr = prop.GetValue(data) as int[];
            if (arr == null || index < 0 || index >= arr.Length) return 0;
            return arr[index];
        }

        [Serializable]
        private class ScaleDataWrapper
        {
            public List<string> ScaleName;
            public List<int> ScalePos;
            public int ScaleNum;
            public int[] ScaleBeat0;
            public int[] ScaleBeat1;
            public int[] ScaleBeat2;
            public int[] ScaleBeat3;
            public int[] ScaleBeat4;
            public int[] ScaleBeat5;
            public int[] ScaleBeat6;
            public int[] ScaleBeat7;
            public int[] ScaleBeat8;
            public int[] ScaleBeat9;
            public int[] ScaleBeat10;
            public int[] ScaleBeat11;
            public int[] ScaleBeat12;
            public int[] ScaleBeat13;
            public int[] ScaleBeat14;
            public int[] ScaleBeat15;
            public int[] ScaleBeat16;
            public int[] ScaleBeat17;
            public int[] ScaleBeat18;
            public int[] ScaleBeat19;
            public int[] ScaleBeat20;
            public int[] ScaleBeat21;
            public int[] ScaleBeat22;
            public int[] ScaleBeat23;
            public int[] ScaleBeat24;
            public int[] ScaleBeat25;
            public int[] ScaleBeat26;
            public int[] ScaleNote0;
            public int[] ScaleNote1;
            public int[] ScaleNote2;
            public int[] ScaleNote3;
            public int[] ScaleNote4;
            public int[] ScaleNote5;
            public int[] ScaleNote6;
            public int[] ScaleNote7;
            public int[] ScaleNote8;
            public int[] ScaleNote9;
            public int[] ScaleNote10;
            public int[] ScaleNote11;
            public int[] ScaleNote12;
            public int[] ScaleNote13;
            public int[] ScaleNote14;
            public int[] ScaleNote15;
            public int[] ScaleNote16;
            public int[] ScaleNote17;
            public int[] ScaleNote18;
            public int[] ScaleNote19;
            public int[] ScaleNote20;
            public int[] ScaleNote21;
            public int[] ScaleNote22;
            public int[] ScaleNote23;
            public int[] ScaleNote24;
            public int[] ScaleNote25;
            public int[] ScaleNote26;
        }

        private class MelodyJsonData
        {
            public string ScaleName;
            public List<NoteJsonData> Notes;
        }

        private class NoteJsonData
        {
            public int ScaleNum;
            public int ScaleBeat;
        }
    }
}