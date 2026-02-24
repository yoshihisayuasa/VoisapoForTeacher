using Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.VolumeControll
{
    /// <summary>
    /// 音量スライダーUI制御
    /// </summary>
    public class VolumeSliderUI : MonoBehaviour
    {
        [SerializeField] private Slider _slider;

        private void Start()
        {
            _slider.value= VolumeManager.Instance?.Volume ?? 1f;
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            VolumeManager.Instance?.SetVolume(value);
        }
    }
}