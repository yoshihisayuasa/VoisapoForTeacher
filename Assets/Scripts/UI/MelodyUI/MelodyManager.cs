using Assets.Scripts.Infrastructure;
using Assets.Scripts.Domain.Entities;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Assets.Scripts.UI.AutoKeyChangeManager;

namespace Assets.Scripts.UI.MelodyUI
{
    public sealed class MelodyManager : MonoBehaviour
    {
        public static MelodyManager Instance { get; private set; }
        public Melody CurrentMelody { get; private set; }

        private readonly Subject<Melody> _melodyChanged = new();
        public Observable<Melody> MelodyChanged => _melodyChanged;

        private const string _jsonFileName = "savedata2";
        private const string defaultMelodyName = "Single";

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
            var loadedEntries = MelodyJsonLoader.LoadFromJsonResource(_jsonFileName);
            if (loadedEntries.Count > 0)
            {
                _melodies.AddRange(loadedEntries);
                CurrentMelody = _melodies.Find(e => e.Melody.Name == defaultMelodyName)?.Melody;
            }
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

        public void ClearCurrentMelody()
        {
            CurrentMelody = null;
        }

        public void ResetToDefault()
        {
            var melody = _melodies.Find(e => e.Melody.Name == defaultMelodyName)?.Melody;
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
                SetCurrentMelody(_melodies.Find(e => e.Melody.Name == defaultMelodyName)?.Melody);
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
            ClearCurrentMelody();
            AutoKeyChangeManager.Instance.SetState(AutoKeyChangeState.None);
            EarphoneModeManager.Instance.SetMode(false);
            SceneManager.LoadScene(sceneName);
        }
    }
}
