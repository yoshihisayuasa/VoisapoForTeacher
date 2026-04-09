using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.MelodyCreate
{
    /// <summary>
    /// +/- ボタンで整数値を増減する汎用スピナー。
    /// </summary>
    public sealed class IntSpinnerUI : MonoBehaviour
    {
        [SerializeField] private Button _plusButton;
        [SerializeField] private Button _minusButton;
        [SerializeField] private TMP_Text _valueText;

        [SerializeField] private int _min = -24;
        [SerializeField] private int _max = 24;
        [SerializeField] private int _initialValue = 0;

        public int Value { get; private set; }

        public event Action<int> OnValueChanged;

        private void Awake()
        {
            Value = _initialValue;
            RefreshText();

            _plusButton.onClick.AddListener(Increment);
            _minusButton.onClick.AddListener(Decrement);
        }

        private void Increment()
        {
            SetValue(Value + 1);
        }

        private void Decrement()
        {
            SetValue(Value - 1);
        }

        private void SetValue(int newValue)
        {
            Value = Mathf.Clamp(newValue, _min, _max);
            RefreshText();
            OnValueChanged?.Invoke(Value);
        }

        private void RefreshText()
        {
            _valueText.text = Value.ToString();
        }
    }
}
