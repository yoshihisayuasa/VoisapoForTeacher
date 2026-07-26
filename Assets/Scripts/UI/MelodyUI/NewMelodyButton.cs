using Assets.Scripts.Domain.Modules;
using Assets.Scripts.Domain.ValueObjects;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyUI
{
    /// <summary>
    /// メロディ作成シーンへ遷移するボタン。作成シーンへの入口をここで一括してゲートする。
    /// </summary>
    public sealed class NewMelodyButton : MonoBehaviour
    {
        [SerializeField] private string _melodyCreateSceneName = "MelodyCreate";
        [SerializeField] private PremiumGateButton _gate;

        private void Start()
        {
            _gate.Bind(new FeatureGate(PremiumFeature.MelodyCreate), OnClicked);
        }

        private void OnClicked()
        {
            MelodyManager.Instance.NavigateToMelodyCreate(_melodyCreateSceneName);
        }
    }
}
