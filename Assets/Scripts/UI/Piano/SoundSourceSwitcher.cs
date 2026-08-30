using System.Collections.Generic;
using Assets.Scripts.Infrastructure;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// ピアノ音源セットの切り替えを管理するクラス。on
    /// </summary>
    public sealed class SoundSourceSwitcher : MonoBehaviour
    {
        public static SoundSourceSwitcher Instance { get; private set; }

        [SerializeField]
        [Tooltip("切り替え可能な音源セット一覧")]
        private List<PianoSoundSet> _soundSets = new();

        private int _currentIndex = 0;

        public int CurrentIndex => _currentIndex;

        private readonly Subject<int> _onChanged = new();
        public Observable<int> OnChanged => _onChanged;

        public float CurrentVolumeMultiplier => _soundSets[_currentIndex].VolumeMultiplier;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
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
            // 保存値は音源セットを減らしたアップデート後に範囲外になり得るため、必ず一覧の範囲へ丸める。
            _currentIndex = Mathf.Clamp(SoundSourcePreference.Recall(0), 0, _soundSets.Count - 1);
            ApplySoundSet(_soundSets[_currentIndex]);
        }

        /// <summary>
        /// 指定インデックスの音源セットに切り替える。
        /// </summary>
        public void SwitchTo(int index)
        {
            _currentIndex = index;
            SoundSourcePreference.Remember(index);
            ApplySoundSet(_soundSets[_currentIndex]);
            _onChanged.OnNext(index);
        }
        private void ApplySoundSet(PianoSoundSet soundSet)
        {
            PianoController.Instance.ApplySoundSet(soundSet);
        }
    }
}
