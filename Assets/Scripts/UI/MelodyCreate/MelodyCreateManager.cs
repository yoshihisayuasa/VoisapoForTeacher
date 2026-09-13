using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.StaticValues;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Infrastructure;
using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.UI.Modal;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// メロディ作成シーンのオーケストレーター。
    /// コード音・メロディステップの編集ロジックを一元管理する。
    /// </summary>
    public sealed class MelodyCreateManager : MonoBehaviour
    {
        public static MelodyCreateManager Instance { get; private set; }

        [SerializeField] private string _mainSceneName = "TeacherMain";
        [SerializeField] private DraftMelodyPlayer _draftMelodyPlayer;

        private MelodyDraft _draft;

        private readonly Subject<Unit> _draftChanged = new();
        public Observable<Unit> DraftChanged => _draftChanged;

        public bool IsChordComplete => _draft.IsChordComplete;
        public bool CanPreview => _draft.CanPreview;
        public bool HasAnyInput => _draft.HasAnyInput;
        public bool CanExtend => _draft.CanExtend;
        public IReadOnlyList<DraftNote> ChordNotes => _draft.ChordNotes;
        public IReadOnlyList<DraftNote> MelodyNotes => _draft.MelodyNotes;
        public int ChordBeats => _draft.ChordBeats;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _draft = new MelodyDraft();

            MelodyTemplateLibrary.Initialize(MelodyTemplateLoader.LoadTemplates());
        }

        /// <summary>
        /// 鍵盤入力を1音追加する。コード音かメロディ音かの振り分けは下書きが判断する。
        /// </summary>
        public void AddNote(PianoNote key)
        {
            _draft.AddNote(key);
            _draftChanged.OnNext(Unit.Default);
        }

        public void ClearAll()
        {
            _draft.ClearAll();
            _draftChanged.OnNext(Unit.Default);
        }

        public void Extend()
        {
            _draft.Extend();
            _draftChanged.OnNext(Unit.Default);
        }

        /// <summary>
        /// 直近の入力を1つ取り消す。
        /// </summary>
        public void DeleteLast()
        {
            _draft.DeleteLast();
            _draftChanged.OnNext(Unit.Default);
        }

        public void LoadTemplate(Melody template)
        {
            _draft.LoadFrom(template, PianoNote.DefaultRoot);
            _draftChanged.OnNext(Unit.Default);
        }

        public void Preview()
        {
            if (!_draft.CanPreview)
            {
                return;
            }
            _draftMelodyPlayer.Play(_draft.Build(string.Empty), _draft.Root);
        }

        public void SaveWithName(string name)
        {
            if (!_draft.CanPreview)
            {
                Debug.LogWarning("メロディが無効です: 和音3音・メロディ1音以上が必要です");
                return;
            }

            if (MelodyManager.Instance.ContainsMelodyWithName(name))
            {
                var body = new LocalizedMessage(
                    japanese: $"「{name}」という名前のメロディはすでにあります。",
                    english: $"A melody named \"{name}\" already exists.");
                ConfirmModalUI.Show(body.ForCurrentLanguage());
                return;
            }

            Melody melody = _draft.Build(name);
            MelodyManager.Instance.AddMelody(melody);

            LoadMainScene();
        }

        public void Home()
        {
            LoadMainScene();
        }

        private void LoadMainScene()
        {
            MelodyPlayer.Instance.StopMelodyAndReset();
            SceneManager.LoadScene(_mainSceneName);
        }
    }
}
