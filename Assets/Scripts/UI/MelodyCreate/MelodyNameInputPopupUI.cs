using UnityEngine;

namespace Assets.Scripts.UI.MelodyCreate
{
    public sealed class MelodyNameInputPopupUI : MonoBehaviour
    {
        public void Open()
        {
            InputModalWindow.Create(ignorable: true)
                .SetInputField(name => MelodyCreateManager.Instance.SaveWithName(name))
                .Show();
        }
    }
}
