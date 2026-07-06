using Assets.Scripts.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Domain.StaticValues
{
    /// <summary>
    /// メロディ作成用テンプレートの保持者。
    /// 読み込み手段は知らず、外（Infrastructure）から渡された一覧を保持するだけ。
    /// </summary>
    public static class MelodyTemplateLibrary
    {
        private static IReadOnlyList<Melody> _templates = Array.Empty<Melody>();

        public static IReadOnlyList<Melody> All => _templates;

        public static void Initialize(IReadOnlyList<Melody> templates)
        {
            _templates = templates ?? Array.Empty<Melody>();
        }
    }
}
