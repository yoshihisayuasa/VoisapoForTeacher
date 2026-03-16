using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BtnOpenModalDialog : MonoBehaviour
{

    // Use this for initialization
    void Awake()
    {
        ModalDialogManager.Instance.DialogClosing += Instance_DialogClosing;

        ModalDialogManager.Instance.DialogClosed += Instance_DialogClosed;
    }

    void Instance_DialogClosed(object sender, Assets.UIModalDialog.Scripts.DialogClosedEventArgs e)
    {
        Debug.Log("Dialog " + e.DialogName + " is closed by button " + e.CloseButton.name);
    }

    void Instance_DialogClosing(object sender, Assets.UIModalDialog.Scripts.DialogClosingEventArgs e)
    {
        if (e.CloseButton.name == "NoButton")
        {
            // Access components of the dialog before closing
            Text errorText = e.DialogPanel.Find("ErrorText").GetComponent<Text>();
            errorText.text = "Can't close dialog, error occured!";
            e.Cancel = true;
        }
    }

    public void ShowDialog()
    {
        ModalDialogManager.Instance.ShowDialog();
    }
}
