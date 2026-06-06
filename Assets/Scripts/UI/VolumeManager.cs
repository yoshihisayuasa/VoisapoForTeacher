using UnityEngine;

namespace AsseScripts.UI
{
    /// <summary>
    /// 音量管理クラス（シングルトン）
    /// </summary>
    public class VolumeManager
    {
        private const string PrefsKey = "Volume";

        public static VolumeManager Instance { get; } = new VolumeManager();
        private float _volume;

        public float Volume => _volume;

        private VolumeManager()
        {
            _volume = PlayerPrefs.GetFloat(PrefsKey, 1f);
        }

        public void SetVolume(float value)
        {
            _volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(PrefsKey, _volume);
            PlayerPrefs.Save();
        }
    }
}