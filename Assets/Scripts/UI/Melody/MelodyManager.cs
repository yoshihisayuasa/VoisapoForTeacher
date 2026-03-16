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

        private readonly string _jsonFileName = "savedata2";


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
            var loadedMelodies = MelodyJsonLoader.LoadFromJsonResource(_jsonFileName);
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

        public void RemoveMelody(DomainMelody melody)
        {
            _melodies.Remove(melody);
            if (CurrentMelody == melody)
            {
                CurrentMelody = null;
                MelodyChanged?.Invoke(null);
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