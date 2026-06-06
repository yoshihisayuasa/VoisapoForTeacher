using AsseScripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 音量スライダーUI制御
    /// </summary>
    public sealed class VolumeSliderUI : MonoBehaviour
    {
        [SerializeField] private Slider _slider;

        private void Start()
        {
            _slider.value= VolumeManager.Instance.Volume;
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            VolumeManager.Instance.SetVolume(value);
        }
    }
}