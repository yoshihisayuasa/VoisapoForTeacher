using Assets.Scripts.Infrastructure;
using Assets.Scripts.Domain.Entities;
using System.Collections.Generic;

namespace Assets.Scripts.Domain.StaticValues
{
    public static class MelodyTemplateLibrary
    {
        public static IReadOnlyList<Melody> All => MelodyTemplateLoader.LoadFromResource("MelodyCreateTemplate");
    }
}
