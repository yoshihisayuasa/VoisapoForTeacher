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

        // レッスン中は画面を触らない時間が長く続くため、自動ロック（スリープ）を止める。
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        InitializeEntitlementAsync().Forget();
    }

#if UNITY_ANDROID || UNITY_EDITOR
    /// <summary>
    /// Android はバックグラウンド復帰後にオーディオ出力が戻らず、以降まったく音が鳴らなくなることがある。
    /// 再開した時点で現在の設定を入れ直し、オーディオシステムを作り直して復帰させる。
    /// フォーカスではなく一時停止を見るのは、通知シェードのような「バックグラウンドへ行っていない
    /// フォーカス喪失」でも Reset が走り、再生中の音を止めてしまうため。
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            return;
        }

        AudioSettings.Reset(AudioSettings.GetConfiguration());
    }
#endif

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
