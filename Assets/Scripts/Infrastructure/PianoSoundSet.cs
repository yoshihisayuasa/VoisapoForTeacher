using System.Collections.Generic;
using Assets.Scripts.Domain.ValueObjects;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    [System.Serializable]
    public class PianoSoundEntry
    {
        public PianoNoteEnum Note;
        public AudioClip Clip;
    }

    /// <summary>
    /// ピアノ音源セット。鍵盤ノートとAudioClipのマッピングを保持する。
    /// </summary>
    [CreateAssetMenu(fileName = "PianoSoundSet", menuName = "Voisapo/PianoSoundSet")]
    public class PianoSoundSet : ScriptableObject
    {
        [SerializeField]
        [Tooltip("音源セット名（UI表示用）")]
        private string _setName;

        [SerializeField]
        [Tooltip("鍵盤ノートとAudioClipの対応一覧")]
        private List<PianoSoundEntry> _entries = new();

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("音量係数（1.0=変更なし）")]
        private float _volumeMultiplier = 1f;

        private Dictionary<PianoNoteEnum, AudioClip> _clipDict;

        public string SetName => _setName;

        public float VolumeMultiplier => _volumeMultiplier;

        private void OnEnable()
        {
            RebuildDict();
        }

        private void RebuildDict()
        {
            _clipDict = new Dictionary<PianoNoteEnum, AudioClip>(_entries.Count);
            foreach (var entry in _entries)
            {
                _clipDict[entry.Note] = entry.Clip;
            }
        }

        /// <summary>
        /// 指定ノートのAudioClipを返す。未登録の場合はnull。
        /// </summary>
        public AudioClip GetClip(PianoNoteEnum note)
        {
            if (_clipDict == null) RebuildDict();
            _clipDict.TryGetValue(note, out var clip);
            return clip;
        }
    }
}
