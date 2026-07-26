using Assets.Scripts;
using Assets.Scripts.Infrastructure;
using Assets.Scripts.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class AppInitializer : MonoBehaviour
{
    [SerializeField] private int _targetFrameRate = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _targetFrameRate;

        InitializeEntitlementAsync().Forget();
    }

    /// <summary>
    /// 課金状態はプレミアム機能を持つ先生ビルドだけで使うため、生徒ビルドではストアへ照会しない。
    /// </summary>
    private async UniTaskVoid InitializeEntitlementAsync()
    {
        if (!AppMode.IsTeacher)
        {
            return;
        }
        await EntitlementManager.Instance.InitializeAsync(PurchaseGatewayFactory.Create());
    }
}
