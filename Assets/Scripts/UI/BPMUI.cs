using AsseScripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BPMUI : MonoBehaviour
{
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;
    [SerializeField] private TMP_Text _bpmText;
    private void Start()
    {
        _plusButton.onClick.AddListener(OnPlusClicked);
        _minusButton.onClick.AddListener(OnMinusClicked);
    }
    private void OnDestroy()
    {
        _plusButton?.onClick.RemoveListener(OnPlusClicked);
        _minusButton?.onClick.RemoveListener(OnMinusClicked);
    }
    private void OnPlusClicked()
    {
        BPMManager.Instance.Increment();
        RefreshBpmText();
    }

    private void OnMinusClicked()
    {
        BPMManager.Instance.Decrement();
        RefreshBpmText();
    }

    private void RefreshBpmText()
    {
        if (_bpmText != null)
        {
            _bpmText.text = BPMManager.Instance.Value.ToString();
        }
    }
}