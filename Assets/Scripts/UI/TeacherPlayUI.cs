using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class TeacherPlayUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _image;

        [Header("Color")]
        [SerializeField] private Color _onColor = Color.red;
        [SerializeField] private Color _offColor = Color.white;
        [SerializeField] private TeacherSideManager _teacherSideManager;

        private void Start()
        {
            if (_toggle == null)
            {
                _toggle = GetComponent<Toggle>();
            }
            if (_image == null)
            {
                var graphic = GetComponent<Selectable>().targetGraphic as Image;
                if (graphic != null)
                {
                    _image = graphic;
                }
            }
            if (_teacherSideManager != null)
            {
                _teacherSideManager.TeacherSideButtonStateChanged += OnTeacherSideButtonStateChangedFromManager;

                // 初期同期
                if (_toggle != null && _toggle.isOn != _teacherSideManager.TeacherSideButtonState)
                {
                    _toggle.isOn = _teacherSideManager.TeacherSideButtonState;
                }
            }

            _toggle.onValueChanged.AddListener(OnToggleChanged);

        }
        private void OnDestroy()
        {
            if (_teacherSideManager != null)
            {
                _teacherSideManager.TeacherSideButtonStateChanged -= OnTeacherSideButtonStateChangedFromManager;
            }
        }
        private void OnToggleChanged(bool isOn)
        {
            if (_image != null)
            {
                _image.color = isOn ? _onColor : _offColor;
            }
                _teacherSideManager.TeacherSideButtonState = isOn;
        }
        private void OnTeacherSideButtonStateChangedFromManager(bool isOn)
        {
            if (_toggle != null && _toggle.isOn != isOn)
            {
                _toggle.isOn = isOn;
            }
        }
    }
}
