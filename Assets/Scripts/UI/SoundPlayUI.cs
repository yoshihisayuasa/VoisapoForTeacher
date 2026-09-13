using Assets.Scripts.UI.Piano;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 音再生側トグルの UI と入力（トグル・Shift・キーリリース）を担当する先生専用クラス。
    /// 実際の状態は <see cref="SoundPlayManager"/> が保持し、ここはそれを駆動・表示するだけ。
    /// 生徒ビルドには配置しない（生徒は受信状態のみで駆動する）。
    /// 生徒が入室していない間はグレーアウトして操作不可にする。
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public sealed class SoundPlayUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Toggle _toggle;
        [SerializeField, FormerlySerializedAs("_image")] private Graphic _graphic;


        private void Start()
        {
            // 先生は毎シーン生成されるため、シーン遷移時は必ずオフから始める。
            SoundPlayManager.Instance.SetTeacherIntent(false);

            _toggle.onValueChanged.AddListener(RequestState);

            PianoController.Instance.OnAnyKeyUpAsObservable
                .Subscribe(_ => RequestState(false))
                .AddTo(this);

            // トグルは先生の「意図」を映す。実効状態（相手がいなければ強制再生）とは分離する。
            SoundPlayManager.Instance.OnIntentChanged
                .Subscribe(SyncToggle)
                .AddTo(this);

            SyncToggle(SoundPlayManager.Instance.TeacherIntent);

            SoundPlayManager.Instance.CanChooseSide
                .Subscribe(SetInteractable)
                .AddTo(this);
        }

        private void SetInteractable(bool canChoose)
        {
            _toggle.interactable = canChoose;
            UpdateVisual();
        }

        private void RequestState(bool value)
        {
            // Shift押下中はオフにしない（押している間だけ鳴らす momentary 操作）。
            if (!value && IsShiftHeld())
            {
                return;
            }

            SoundPlayManager.Instance.SetTeacherIntent(value);
        }

        private void SyncToggle(bool isOn)
        {
            if (_toggle.isOn != isOn)
            {
                _toggle.isOn = isOn;
            }
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            _graphic.color = _toggle.interactable
                ? AppColors.ActiveOrWhite(_toggle.isOn)
                : AppColors.Disabled;
        }

        private void Update()
        {
            if (IsShiftPressedThisFrame())       RequestState(true);
            else if (IsShiftReleasedThisFrame()) RequestState(false);
        }

        private static bool IsShiftHeld()
        {
            var kb = Keyboard.current;
            return kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
        }

        private static bool IsShiftPressedThisFrame()
        {
            var kb = Keyboard.current;
            return kb != null && (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame);
        }

        private static bool IsShiftReleasedThisFrame()
        {
            var kb = Keyboard.current;
            return kb != null && (kb.leftShiftKey.wasReleasedThisFrame || kb.rightShiftKey.wasReleasedThisFrame);
        }
    }
}
