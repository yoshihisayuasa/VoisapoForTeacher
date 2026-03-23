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

        private const string _jsonFileName = "savedata2";
        private const string defaultMelodyName = "Single";


        private List<DomainMelody> _melodies = new();
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            var loadedMelodies = MelodyJsonLoader.LoadFromJsonResource(_jsonFileName);
            if (loadedMelodies.Count > 0)
            {
                _melodies.AddRange(loadedMelodies);
                CurrentMelody = _melodies.Find(m => m.Name == defaultMelodyName);
            }
        }

        public IReadOnlyList<DomainMelody> GetAllMelodies() => _melodies;
        public void AddMelody(DomainMelody melody)
        {
            _melodies.Add(melody);
            SaveAllMelodies();
        }

        public void SetCurrentMelody(DomainMelody melody)
        {
            CurrentMelody = melody;
            MelodyChanged?.Invoke(melody);
        }

        public void RemoveMelody(DomainMelody melody)
        {
            _melodies.Remove(melody);
            if (CurrentMelody == melody)
            {
                CurrentMelody = _melodies.Find(m => m.Name == defaultMelodyName);
                MelodyChanged?.Invoke(CurrentMelody);
            }
            SaveAllMelodies();
        }
        public void SavePositions()
        {
            MelodyJsonLoader.SavePositions(_jsonFileName, _melodies);
        }

        public void SaveAllMelodies()
        { 
            MelodyJsonLoader.SaveAllMelodies(_jsonFileName, _melodies);
        }
    }
}