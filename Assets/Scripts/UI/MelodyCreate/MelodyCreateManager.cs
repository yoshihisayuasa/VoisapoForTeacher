using System;
using AsseScripts.Domain;
using Assets.Scripts.UI.Melody;
using UnityEngine;
using UnityEngine.SceneManagement;
using DomainPianoNote = AsseScripts.Domain.PianoNote;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// メロディ作成シーンのオーケストレーター。
    /// </summary>
    public sealed class MelodyCreateManager : MonoBehaviour
    {
        public static MelodyCreateManager Instance { get; private set; }

        [SerializeField] private string _mainSceneName = "Main";

        public MelodyDraft Draft { get; private set; }

        public event Action DraftChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Draft = new MelodyDraft();

            MelodyPlayer.Instance.BlockInput = true;
        }

        private void OnDestroy()
        {
            if (MelodyPlayer.Instance != null)
            {
                MelodyPlayer.Instance.BlockInput = false;
            }
        }

        public void SetName(string name)
        {
            Draft.Name = name;
            DraftChanged?.Invoke();
        }

        public void SetChordNote(int index, DomainPianoNote key)
        {
            Draft.SetChordNote(index, key);
            DraftChanged?.Invoke();
        }

        public void AddMelodyStep(StepEntry entry)
        {
            Draft.AddMelodyStep(entry);
            DraftChanged?.Invoke();
        }

        public void RemoveLastMelodyStep()
        {
            Draft.RemoveLastMelodyStep();
            DraftChanged?.Invoke();
        }

        public void Save()
        {
            if (!Draft.IsValid)
            {
                Debug.LogWarning("メロディが無効です: 名前・和音3音・メロディ1音以上が必要です");
                return;
            }

            int position = MelodyManager.Instance.GetAllMelodies().Count;
            var melody = Draft.Build(position);
            MelodyManager.Instance.AddMelody(melody);

            LoadMainScene();
        }

        public void Cancel()
        {
            LoadMainScene();
        }

        private void LoadMainScene()
        {
            MelodyPlayer.Instance.BlockInput = false;
            SceneManager.LoadScene(_mainSceneName);
        }
    }
}
