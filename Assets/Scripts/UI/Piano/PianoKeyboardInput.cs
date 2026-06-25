using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// キーボードによる鍵盤操作（選択移動・選択中鍵盤の再生／離鍵）。
    /// 先生ビルドにのみ存在し、生徒ビルドではAwakeで自身を破棄する。
    /// </summary>
    public sealed class PianoKeyboardInput : MonoBehaviour
    {
        private void Awake()
        {
            if (!AppMode.IsTeacher)
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            var piano = PianoController.Instance;

            if (kb.sKey.wasPressedThisFrame || kb.jKey.wasPressedThisFrame) piano.MoveSelection(-1);
            if (kb.fKey.wasPressedThisFrame || kb.lKey.wasPressedThisFrame) piano.MoveSelection(1);
            if (kb.gKey.wasPressedThisFrame || kb.semicolonKey.wasPressedThisFrame) piano.MoveSelection(-6);
            if (kb.aKey.wasPressedThisFrame || kb.hKey.wasPressedThisFrame) piano.MoveSelection(6);

            var selected = piano.SelectedKey;
            if (selected == null) return;

            if (kb.dKey.wasPressedThisFrame || kb.kKey.wasPressedThisFrame) piano.PressKey(selected);
            if (kb.dKey.wasReleasedThisFrame || kb.kKey.wasReleasedThisFrame) piano.ReleaseKey(selected);
        }
    }
}
