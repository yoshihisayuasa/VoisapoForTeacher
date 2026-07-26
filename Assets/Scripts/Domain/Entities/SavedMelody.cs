using Assets.Scripts.Domain.ValueObjects;
using System;

namespace Assets.Scripts.Domain.Entities
{
    public sealed class SavedMelody
    {
        public Melody Melody { get; }
        public int Position { get; private set; }

        public bool IsProtected => Melody.IsProtected;

        public SavedMelody(Melody melody, int position)
        {
            Melody = melody;
            Position = position;
        }

        public void SetPosition(int position) => Position = position;

        /// <summary>
        /// このメロディを選択して良いかを課金状態に照らして判断する。判断はメロディ自身が持つ。
        /// </summary>
        public void GateSelect(Entitlement entitlement, Action onAllowed, Action onDenied)
        {
            Melody.GateSelect(entitlement, onAllowed, onDenied);
        }
    }
}
