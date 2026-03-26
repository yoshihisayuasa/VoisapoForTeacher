using System.Collections.Generic;
using AsseScripts.Infrastructure;
using UnityEngine;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// ピアノ音源セットの切り替えを管理するクラス。on
    /// </summary>
    public class SoundSourceSwitcher : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("切り替え可能な音源セット一覧")]
        private List<PianoSoundSet> _soundSets = new();

        private int _currentIndex = 1;

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
