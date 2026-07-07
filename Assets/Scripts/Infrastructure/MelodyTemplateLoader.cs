using Assets.Scripts.Domain.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    public static class MelodyTemplateLoader
    {
        private const string ResourceFileName = "MelodyCreateTemplate";

        public static IReadOnlyList<Melody> LoadTemplates()
        {
            var asset = Resources.Load<TextAsset>(ResourceFileName);
            if (asset == null)
            {
                Debug.LogError($"テンプレートJSONのロードに失敗: {ResourceFileName}");
                return new List<Melody>();
            }

            try
            {
                var wrapper = JsonUtility.FromJson<MelodyListWrapper>(asset.text);
                return Parse(wrapper);
            }
            catch (Exception ex)
            {
                Debug.LogError($"テンプレートJSONパースエラー: {ex.Message}");
                return new List<Melody>();
            }
        }

        private static List<Melody> Parse(MelodyListWrapper wrapper)
        {
            var result = new List<Melody>();
            if (wrapper.Melodies == null)
            {
                return result;
            }
            foreach (var data in wrapper.Melodies)
            {
                if (string.IsNullOrEmpty(data.Name)) continue;
                result.Add(MelodyJsonConverter.ToMelody(data));
            }
            return result;
        }
    }
}
