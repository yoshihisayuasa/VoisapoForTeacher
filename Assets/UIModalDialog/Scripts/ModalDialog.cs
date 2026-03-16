using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class ModalDialog : MonoBehaviour
{

    public Button[] _submitButtons;

    void OnEnable()
    {
        transform.SetAsLastSibling();
    }

    public void Show()
    {
        gameObject.SetActive(true);

        //// Bind button
        foreach (Button button in _submitButtons)
        {
            BindButton(button);
        }
    }

    private void BindButton(Button button)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { Close(button); });
    }

    public void Close(Button button)
    {
        ModalDialogManager.Instance.CloseDialog(button);
    }
}
