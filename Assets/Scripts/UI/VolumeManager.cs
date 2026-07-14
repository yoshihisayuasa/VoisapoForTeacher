using Assets.Scripts.Domain.ValueObjects;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 音量管理クラス（シングルトン）
    /// </summary>
    public class VolumeManager
    {
        private const string PrefsKey = "Volume";

        public static VolumeManager Instance { get; } = new VolumeManager();
        private Volume _volume;

        public Volume Volume => _volume;

        private VolumeManager()
        {
            _volume = new Volume(PlayerPrefs.GetFloat(PrefsKey, 1f));
        }

        public void SetVolume(float value)
        {
            _volume = new Volume(value);
            PlayerPrefs.SetFloat(PrefsKey, _volume.Value);
            PlayerPrefs.Save();
        }
    }
}
