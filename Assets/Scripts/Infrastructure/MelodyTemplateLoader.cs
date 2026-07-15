using Assets.Scripts.Domain.Entities;
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

            var templates = new List<Melody>();
            foreach (var entry in MelodyJsonConverter.ToSavedMelodies(asset.text, "テンプレートJSON"))
            {
                templates.Add(entry.Melody);
            }
            return templates;
        }
    }
}
