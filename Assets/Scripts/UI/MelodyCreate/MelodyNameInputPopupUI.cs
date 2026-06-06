using System;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyCreate
{
    public sealed class MelodyNameInputPopupUI : MonoBehaviour
    {
        public void Open(Action<string> onNameEntered)
        {
            InputModalWindow.Create(ignorable: true)
                .SetInputField(onNameEntered)
                .Show();
        }
    }
}
