using Assets.Scripts.Domain.Entities;
using Assets.Scripts.Domain.ValueObjects;
using System;

namespace Assets.Scripts.Domain.Modules
{
    /// <summary>
    /// メロディ選択のゲート。可否はメロディの由来によって変わるため、
    /// 判断そのものは <see cref="SavedMelody"/> に委ねる。
    /// </summary>
    public sealed class MelodySelectGate : IPremiumGate
    {
        private readonly SavedMelody _melody;

        public MelodySelectGate(SavedMelody melody)
        {
            _melody = melody;
        }

        public void Gate(Entitlement entitlement, Action onAllowed, Action onDenied)
        {
            _melody.GateSelect(entitlement, onAllowed, onDenied);
        }
    }
}
