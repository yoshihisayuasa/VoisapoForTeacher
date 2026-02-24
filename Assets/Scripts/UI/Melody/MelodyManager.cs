using Scripts.Infrastructure;
using System;
using System.Collections.Generic;
using UnityEngine;
using DomainMelody = Scripts.Domain.Melody;

namespace Scripts.UI.Melody
{
    public class MelodyManager : MonoBehaviour
    {
        public static MelodyManager Instance { get; private set; }
        public DomainMelody CurrentMelody { get; private set; }

        public event Action<DomainMelody> MelodyChanged;

        private List<DomainMelody> _melodies = new();
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            var loadedMelodies = MelodyJsonLoader.LoadFromJsonResource("savedata2");
            if (loadedMelodies.Count > 0)
            {
                _melodies.AddRange(loadedMelodies);
                CurrentMelody = _melodies[2];
            }
        }

        public IReadOnlyList<DomainMelody> GetAllMelodies() => _melodies;
        public void AddMelody(DomainMelody melody) => _melodies.Add(melody);

        public void SetCurrentMelody(DomainMelody melody)
        {
            if (melody == null)
            {
                return;
            }
            CurrentMelody = melody;
            MelodyChanged?.Invoke(melody);
        }

   
        public void SetCurrentMelodyByIndex(int index)
        {
            if (0 <= index && index < _melodies.Count)
            {
                SetCurrentMelody(_melodies[index]);
            }
        }
        public bool TryGetMelody(int index, out DomainMelody melody)
        {
            if (0 <= index && index < _melodies.Count)
            {
                melody = _melodies[index];
                return true;
            }
            melody = null;
            return false;
        }
    }

    [System.Serializable]
    public class MelodyDataWrapper
    {
        public List<MelodyJson> Melodies;
    }

    [System.Serializable]
    public class MelodyJson
    {
        public string ScaleName;
        public int ScaleNum;
        public List<int> ScaleNoteX;
        public List<float> ScaleBeatX;
    }
}