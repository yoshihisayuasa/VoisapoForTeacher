using Assets.Scripts.Infrastructure;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI.MelodyUI
{
    public sealed class MelodyManager : MonoBehaviour
    {
        public static MelodyManager Instance { get; private set; }
        public Melody CurrentMelody { get; private set; }

        private readonly Subject<Melody> _melodyChanged = new();
        public Observable<Melody> MelodyChanged => _melodyChanged;

        private const string _jsonFileName = "savedata3";
        private const string _legacyJsonFileName = "savedata2";

        private readonly List<SavedMelody> _melodies = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                PersistentRegistry.Register(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            var loadedEntries = MelodyJsonLoader.LoadFromJsonResource(_jsonFileName, _legacyJsonFileName);
            if (loadedEntries.Count > 0)
            {
                _melodies.AddRange(loadedEntries);
                CurrentMelody = FindDefaultMelody();
            }
        }

        private Melody FindDefaultMelody()
        {
            return _melodies[0].Melody;
        }

        public IReadOnlyList<SavedMelody> GetAllMelodies() => _melodies;
        public int MelodyCount => _melodies.Count;
        public bool ContainsMelodyWithName(string name) => _melodies.Exists(e => e.Melody.Name == name);

        public void AddMelody(SavedMelody entry)
        {
            _melodies.Add(entry);
            SaveAllMelodies();
        }

        public void SetCurrentMelody(Melody melody)
        {
            CurrentMelody = melody;
            _melodyChanged.OnNext(melody);
        }

        public void ResetToDefault()
        {
            var melody = FindDefaultMelody();
            if (melody != null)
            {
                SetCurrentMelody(melody);
            }
        }

        public void RemoveMelody(SavedMelody entry)
        {
            _melodies.Remove(entry);
            if (CurrentMelody == entry.Melody)
            {
                SetCurrentMelody(FindDefaultMelody());
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

        public void NavigateToMelodyCreate(string sceneName)
        {
            MelodyPlayer.Instance.StopMelodyAndReset();
            CurrentMelody = null;
            AutoKeyChangeManager.Instance.SetState(AutoKeyChangeState.None);
            EarphoneModeManager.Instance.SetMode(false);
            SceneManager.LoadScene(sceneName);
        }
    }
}
