using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI.Modal;
using Cysharp.Threading.Tasks;
using R3;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 設定画面のサブスクリプション欄。
    /// 解約とプラン変更はストアの管理画面で行うため、アプリ内では案内しか出さない。
    /// </summary>
    public sealed class SubscriptionSettingsUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Button _manageButton;
        [SerializeField] private Button _restoreButton;
        [SerializeField] private UrlOpener _urlOpener;

        private void Start()
        {
            _purchaseButton.onClick.AddListener(PremiumPurchaseModalUI.Show);
            _manageButton.onClick.AddListener(OpenManagementPage);
            _restoreButton.onClick.AddListener(() => RestoreAsync().Forget());

            EntitlementManager.Instance.Current.Subscribe(ShowStatus).AddTo(this);
        }

        private void ShowStatus(Entitlement entitlement)
        {
            entitlement.Describe(ShowFree, ShowPremium);
        }

        private void ShowFree()
        {
            _statusText.text = "Free";
            _purchaseButton.gameObject.SetActive(true);
        }

        private void ShowPremium(DateTimeOffset expiresAt)
        {
            _statusText.text = $"Premium (until {expiresAt.ToLocalTime():yyyy/MM/dd})";
            _purchaseButton.gameObject.SetActive(false);
        }

        private void OpenManagementPage()
        {
            _urlOpener.OpenUrl(EntitlementManager.Instance.ManagementUrl);
        }

        private async UniTaskVoid RestoreAsync()
        {
            _restoreButton.interactable = false;
            await EntitlementManager.Instance.RestoreAsync();
            _restoreButton.interactable = true;
        }
    }
}
