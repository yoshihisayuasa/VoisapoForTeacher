using System.Collections.Generic;
using AsseScripts.Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// ピアノ音源セットの切り替えを管理するクラス。on
    /// </summary>
    public class SoundSourceSwitcher : MonoBehaviour
    {
        public static SoundSourceSwitcher Instance { get; private set; }

        [SerializeField]
        [Tooltip("切り替え可能な音源セット一覧")]
        private List<PianoSoundSet> _soundSets = new();

        private int _currentIndex = 0;

        public int CurrentIndex => _currentIndex;

        public float CurrentVolumeMultiplier => _soundSets[_currentIndex].VolumeMultiplier;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (PianoController.Instance == null)
            {
                return;
            }
            ApplySoundSet(_soundSets[_currentIndex]);
        }

        public IReadOnlyList<string> SoundSetNames
        {
            get
            {
                var names = new List<string>(_soundSets.Count);
                foreach (var set in _soundSets)
                {
                    names.Add(set.SetName);
                }
                return names;
            }
        }

        private void Start()
        {
            ApplySoundSet(_soundSets[_currentIndex]);
        }

        /// <summary>
        /// 指定インデックスの音源セットに切り替える。
        /// </summary>
        public void SwitchTo(int index)
        {
            _currentIndex = index;
            ApplySoundSet(_soundSets[_currentIndex]);
        }
        private void ApplySoundSet(PianoSoundSet soundSet)
        {
            var controller = Assets.Scripts.UI.Piano.PianoController.Instance;

            foreach (var keyUI in controller.PianoKeys)
            {
                var clip = soundSet.GetClip(keyUI.NoteEnum);
                keyUI.SwapAudioClip(clip);
            }
        }
    }
}
