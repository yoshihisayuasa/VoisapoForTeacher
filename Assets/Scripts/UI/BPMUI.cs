using Assets.Scripts.UI;
using R3;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// BPMの増減UI。ボタンに加えて R/I で増加、E/U で減少のショートカットを受け付ける。
/// 先生シーンにのみ存在する。
/// </summary>
public sealed class BPMUI : MonoBehaviour
{
    [SerializeField] private Button _plusButton;
    [SerializeField] private Button _minusButton;
    [SerializeField] private TMP_Text _bpmText;

    private const float BlinkDuration = 5f;
    private Coroutine _blinkCoroutine;

    private void Start()
    {
        _plusButton.onClick.AddListener(IncrementBpm);
        _minusButton.onClick.AddListener(DecrementBpm);

        // 先生からの同期受信を含むBPM変更をテキストへ反映する。
        BPMManager.Instance.BpmChanged
            .Subscribe(_ => RefreshBpmText())
            .AddTo(this);

        RefreshBpmText();
    }

    private void OnDestroy()
    {
        _plusButton.onClick.RemoveListener(IncrementBpm);
        _minusButton.onClick.RemoveListener(DecrementBpm);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.rKey.wasPressedThisFrame || kb.iKey.wasPressedThisFrame) IncrementBpm();
        if (kb.eKey.wasPressedThisFrame || kb.uKey.wasPressedThisFrame) DecrementBpm();
    }

    private void IncrementBpm()
    {
        BPMManager.Instance.Increment();
        StartBlink();
    }

    private void DecrementBpm()
    {
        BPMManager.Instance.Decrement();
        StartBlink();
    }

    private void RefreshBpmText()
    {
        _bpmText.text = BPMManager.Instance.Value.ToString();
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