using Assets.Scripts.Domain.ValueObjects;
using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// 最後にストアから取得した課金状態（期限含む）の永続キャッシュ。
    /// オフライン起動時の初期値に使うだけで、正はあくまで起動時のストア照会。
    /// 期限を過ぎていればプレミアムとしては復元されないため、
    /// オフラインのまま期限切れになったユーザーは Free に落ちる。
    /// </summary>
    public static class EntitlementCache
    {
        private const string FileName = "entitlement";

        public static void Save(Entitlement entitlement)
        {
            entitlement.Describe(
                onFree: () => Write(0),
                onPremium: expiresAt => Write(expiresAt.UtcTicks));
        }

        public static Entitlement Load()
        {
            string path = GetPath();
            if (!System.IO.File.Exists(path))
            {
                return Entitlement.Free;
            }

            try
            {
                var data = JsonUtility.FromJson<EntitlementData>(System.IO.File.ReadAllText(path));
                if (data.ExpiresAtUtcTicks <= 0)
                {
                    return Entitlement.Free;
                }
                return Entitlement.PremiumUntil(new DateTimeOffset(data.ExpiresAtUtcTicks, TimeSpan.Zero));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Entitlement] キャッシュの読み込みに失敗: {ex.Message}");
                return Entitlement.Free;
            }
        }

        private static void Write(long expiresAtUtcTicks)
        {
            try
            {
                var data = new EntitlementData { ExpiresAtUtcTicks = expiresAtUtcTicks };
                System.IO.File.WriteAllText(GetPath(), JsonUtility.ToJson(data, true));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Entitlement] キャッシュの保存に失敗: {ex.Message}");
            }
        }

        private static string GetPath()
            => System.IO.Path.Combine(Application.persistentDataPath, FileName + ".json");

        [Serializable]
        private class EntitlementData
        {
            public long ExpiresAtUtcTicks;
        }
    }
}
