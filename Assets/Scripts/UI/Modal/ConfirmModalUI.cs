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

        /// <summary>ウィンドウ幅の上限（コンテナ幅に対する比率）。画面からはみ出させない。</summary>
        private const float MaximumWidthRatio = 0.9f;

        [SerializeField] private RectTransform _container;
        [SerializeField] private RectTransform _window;
        [SerializeField] private RectTransform _bodyRect;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _cancelButton;

        private Action _onConfirm;
        private Action _onCancel;

        private Vector2 _baseWindowSize;
        private Vector2 _baseBodySize;

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
        }

        /// <summary>
        /// 本文に合わせてウィンドウの大きさを決める。
        /// 1行で収まるうちは横に伸ばし、コンテナ幅の上限に達したら折り返して縦に伸ばす。
        /// </summary>
        private void ResizeToBody(string body)
        {
            var maximumWidth = _container.rect.width * MaximumWidthRatio;

            _bodyText.textWrappingMode = TextWrappingModes.NoWrap;
            var windowWidth = _bodyText.GetPreferredValues(body).x + HorizontalPadding;

            if (windowWidth > maximumWidth)
            {
                _bodyText.textWrappingMode = TextWrappingModes.Normal;
                windowWidth = maximumWidth;
            }

            var bodyWidth = windowWidth - HorizontalPadding;
            var bodyHeight = _bodyText.GetPreferredValues(body, bodyWidth, 0f).y;
            var extraHeight = Mathf.Max(0f, bodyHeight - _baseBodySize.y);

            _window.sizeDelta = new Vector2(windowWidth, _baseWindowSize.y + extraHeight);
            _bodyRect.sizeDelta = new Vector2(_baseBodySize.x, _baseBodySize.y + extraHeight);
        }

        private void Awake()
        {
            _baseWindowSize = _window.sizeDelta;
            _baseBodySize = _bodyRect.sizeDelta;
        }

        /// <summary>
        /// ModalContainer は onLoad（= Setup）を親付けより前に呼ぶため、
        /// その時点ではコンテナ幅が 0 で上限を決められない。大きさの調整はここで行う。
        /// </summary>
        private void Start()
        {
            _okButton.onClick.AddListener(OnOkClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);

            ResizeToBody(_bodyText.text);
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
