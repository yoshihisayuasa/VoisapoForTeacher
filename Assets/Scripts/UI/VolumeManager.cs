using UnityEngine;

namespace Scripts.UI
{
    /// <summary>
    /// 音量管理クラス（シングルトン）
    /// </summary>
    public class VolumeManager 
    {
        public static VolumeManager Instance { get; } = new VolumeManager();
        private float _volume = 1f;

        public float Volume=> _volume; // 音量プロパティ

        public void SetVolume(float value)
        {
            _volume = Mathf.Clamp01(value);
        }
    }
}