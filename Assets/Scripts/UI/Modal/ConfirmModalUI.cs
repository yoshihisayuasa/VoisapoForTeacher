using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace Assets.Scripts.UI.Modal
{
    /// <summary>
    /// 本文・OK/Cancel を表示する汎用確認モーダル。
    /// Cancel コールバック未指定（単一ボタン）の場合は Cancel ボタンを隠す。
    /// </summary>
    public sealed class ConfirmModalUI : MonoBehaviour
    {
        private const string ResourcePath =
            "Prefab/UnityScreenNavigator/Modal/pfb_ui_modal_confirm";

        /// <summary>本文左右の余白（px）。本文の希望幅に加えてウィンドウ幅を決める。</summary>
        private const float HorizontalPadding = 64f;

        [SerializeField] private RectTransform _window;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _cancelButton;

        private Action _onConfirm;
        private Action _onCancel;

        /// <summary>
        /// 確認モーダルを表示する。onCancel が null のときは OK のみ（情報表示）になる。
        /// </summary>
        public static void Show(string body, Action onConfirm = null, Action onCancel = null)
        {
            ModalContainer.Find("ModalContainer").Push(ResourcePath, true, onLoad: x =>
            {
                x.modal.GetComponentInChildren<ConfirmModalUI>().Setup(body, onConfirm, onCancel);
            });
        }

        /// <summary>
        /// 表示内容と押下時の振る舞いを設定する。onCancel が null のときは Cancel ボタンを隠す。
        /// </summary>
        public void Setup(string body, Action onConfirm, Action onCancel = null)
        {
            _bodyText.text = body;
            _onConfirm = onConfirm;
            _onCancel = onCancel;

            _cancelButton.gameObject.SetActive(onCancel != null);

            ResizeWidthToBody(body);
        }

        /// <summary>
        /// 本文の希望幅にパディングを足してウィンドウ横幅を設定する。
        /// 高さは変えず、メッセージに応じて横幅だけ伸縮させる（常に1行）。
        /// </summary>
        private void ResizeWidthToBody(string body)
        {
            _bodyText.textWrappingMode = TextWrappingModes.NoWrap;

            var bodyWidth = _bodyText.GetPreferredValues(body).x;

            _window.sizeDelta = new Vector2(bodyWidth + HorizontalPadding, _window.sizeDelta.y);
        }

        private void Start()
        {
            _okButton.onClick.AddListener(OnOkClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnOkClicked()
        {
            _onConfirm?.Invoke();
            ModalContainer.Of(transform).Pop(true);
        }

        private void OnCancelClicked()
        {
            _onCancel?.Invoke();
            ModalContainer.Of(transform).Pop(true);
        }
    }
}
