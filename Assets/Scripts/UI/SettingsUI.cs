using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Piano;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace Assets.Scripts.UI
{
    public sealed class SettingsUI : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _recordingDeviceDropdown;
        [SerializeField] private TMP_Dropdown _soundSourceDropdown;
        [SerializeField] private Slider _pianoScaleSlider;
        [SerializeField] private Button _closeButton;

        private void Start()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            InitializeRecordingDeviceDropdown();
            InitializeSoundSourceDropdown();
            InitializePianoScaleSlider();
        }

        private void OnCloseButtonClicked()
        {
            ModalContainer.Of(transform).Pop(true);
        }

        private void InitializeRecordingDeviceDropdown()
        {
            //生徒の場合はnull
            if (_recordingDeviceDropdown == null)
            {
                return;
            }
            _recordingDeviceDropdown.ClearOptions();

            var devices = PhraseRecorder.Instance.AvailableDevices;
            foreach (var device in devices)
            {
                _recordingDeviceDropdown.options.Add(new TMP_Dropdown.OptionData(device));
            }

            _recordingDeviceDropdown.RefreshShownValue();
            _recordingDeviceDropdown.onValueChanged.AddListener(OnRecordingDeviceSelected);

            if (devices.Length > 0)
            {
                PhraseRecorder.Instance.SelectDevice(devices[0]);
            }
        }

        private void InitializeSoundSourceDropdown()
        {
            if (_soundSourceDropdown == null)
            {
                return;
            }
            _soundSourceDropdown.ClearOptions();

            foreach (var name in SoundSourceSwitcher.Instance.SoundSetNames)
            {
                _soundSourceDropdown.options.Add(new TMP_Dropdown.OptionData(name));
            }

            _soundSourceDropdown.value = SoundSourceSwitcher.Instance.CurrentIndex;
            _soundSourceDropdown.RefreshShownValue();
            _soundSourceDropdown.onValueChanged.AddListener(OnSoundSourceSelected);
        }

        private void OnRecordingDeviceSelected(int index)
        {
            var devices = PhraseRecorder.Instance.AvailableDevices;
            if (index >= 0 && index < devices.Length)
            {
                PhraseRecorder.Instance.SelectDevice(devices[index]);
            }
        }

        private void OnSoundSourceSelected(int index)
        {
            SoundSourceSwitcher.Instance.SwitchTo(index);
        }

        private void InitializePianoScaleSlider()
        {
            _pianoScaleSlider.minValue = PianoScale.Min;
            _pianoScaleSlider.maxValue = PianoScale.Max;
            _pianoScaleSlider.value = PianoScaleManager.Instance.Scale.Value;
            _pianoScaleSlider.onValueChanged.AddListener(PianoScaleManager.Instance.SetScale);
        }
    }
}