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
            InitializeRecordingDeviceDropdown();
            InitializeSoundSourceDropdown();
            InitializePianoScaleSlider();
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnCloseButtonClicked()
        {
            ModalContainer.Of(transform).Pop(true);
        }

        private void InitializeRecordingDeviceDropdown()
        {
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
            if (_pianoScaleSlider == null)
            {
                return;
            }

            var zoomController = FindObjectOfType<PianoZoomController>();
            if (zoomController == null)
            {
                return;
            }

            _pianoScaleSlider.minValue = zoomController.MinScale;
            _pianoScaleSlider.maxValue = zoomController.MaxScale;
            _pianoScaleSlider.value = zoomController.CurrentScale;
            _pianoScaleSlider.onValueChanged.AddListener(value => zoomController.SetScale(value));
        }

    }
}
