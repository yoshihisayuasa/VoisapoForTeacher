using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Domain.Entities;
using Assets.Scripts.UI.Piano;
using System.Collections;

namespace Assets.Scripts.UI.MelodyUI.PlayStrategies
{
    /// <summary>
    /// メロディ種別（MelodyKind）ごとの再生手順。
    /// </summary>
    public interface IMelodyPlayStrategy
    {
        bool SupportAutoKeyChange { get; }
        bool StopOnKeyUp { get; }

        IEnumerator Execute(PianoController piano, Melody melody,
                            PianoNote pressedKey, PlayModeSettings settings);
    }
}
