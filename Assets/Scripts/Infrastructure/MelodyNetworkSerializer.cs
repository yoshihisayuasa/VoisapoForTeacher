using Assets.Scripts.Domain.Entities;
using System;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// 単一メロディのネットワーク送受信用シリアライズ。
    /// 保存ファイル（曲リスト）とはフォーマットを共有するが用途が別なので、変換の入口は分けて持つ。
    /// </summary>
    public static class MelodyNetworkSerializer
    {
        /// <summary>
        /// 単一メロディを JSON 文字列へシリアライズする（送信用）。
        /// </summary>
        public static string Serialize(Melody melody)
        {
            return JsonUtility.ToJson(MelodyJsonConverter.ToData(melody, 0));
        }

        /// <summary>
        /// JSON 文字列から単一メロディを復元する（受信用）。失敗時は null。
        /// </summary>
        public static Melody Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var data = JsonUtility.FromJson<MelodyData>(json);
                if (data == null || string.IsNullOrEmpty(data.Name)) return null;
                return MelodyJsonConverter.ToMelody(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"メロディの復元に失敗: {ex.Message}");
                return null;
            }
        }
    }
}
