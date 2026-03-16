using UnityEngine;
using System.Collections;

public class OpenSocialTimer : MonoBehaviour
{
    public float TimeToOpen = 2;

    // Use this for initialization
    void Awake()
    {
        Invoke("ShowDialog", TimeToOpen);

        ModalDialogManager.Instance.DialogClosed += Instance_DialogClosed;
    }

    void Instance_DialogClosed(object sender, Assets.UIModalDialog.Scripts.DialogClosedEventArgs e)
    {
        if (e.CloseButton.name == "SubscribeButton")
        {
            Application.OpenURL("http://www.youtube.com/subscription_center?add_user=jayanamgames");
        }
    }

    public void ShowDialog()
    {
        ModalDialogManager.Instance.ShowDialog();
    }
}
