using AsseScripts.UI;
using Assets.Scripts.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BPMUI : MonoBehaviour
{
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;
    [SerializeField] private TMP_Text _bpmText;

    private const float BlinkDuration = 5f;
    private Coroutine _blinkCoroutine;

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
        StartBlink();
    }

    private void OnMinusClicked()
    {
        BPMManager.Instance.Decrement();
        RefreshBpmText();
        StartBlink();
    }

    private void RefreshBpmText()
    {
        if (_bpmText != null)
        {
            _bpmText.text = BPMManager.Instance.Value.ToString();
        }
    }

    private void StartBlink()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        float elapsed = 0f;
        float beatInterval = BPMManager.Instance.SecondPerBeat;
        bool visible = true;

        while (elapsed < BlinkDuration)
        {
            visible = !visible;
            _bpmText.color = visible
                ? AppColors.PianoKeyPlaying
                : Color.white;

            yield return new WaitForSeconds(beatInterval / 2f);
            elapsed += beatInterval / 2f;
        }

        _bpmText.color = Color.white;

        _blinkCoroutine = null;
    }
}