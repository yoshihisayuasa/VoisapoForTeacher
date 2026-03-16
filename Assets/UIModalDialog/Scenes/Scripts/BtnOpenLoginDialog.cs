using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BtnOpenLoginDialog : MonoBehaviour {

    // Use this for initialization
    void Awake()
    {
        ModalDialogManager.Instance.DialogOpened += Instance_DialogOpened;

        ModalDialogManager.Instance.DialogClosing += Instance_DialogClosing;

        ModalDialogManager.Instance.DialogClosed += Instance_DialogClosed;
    }

    void Instance_DialogOpened(object sender, Assets.UIModalDialog.Scripts.DialogEventArgs e)
    {
        LoginCallback loginCallBack = e.DialogPanel.GetComponent<LoginCallback>();
        loginCallBack.ClearInput();
    }

    void Instance_DialogClosed(object sender, Assets.UIModalDialog.Scripts.DialogClosedEventArgs e)
    {
        if (e.CloseButton.name == "LoginButton")
        {
            Debug.Log("Ok, Login successfull");
        }
    }

    void Instance_DialogClosing(object sender, Assets.UIModalDialog.Scripts.DialogClosingEventArgs e)
    {
        if (e.CloseButton.name == "LoginButton")
        {
            // Access Login callback scripton Login
            LoginCallback loginCallback = e.DialogPanel.GetComponent<LoginCallback>();
            loginCallback.ClearErrors();

            if(String.IsNullOrEmpty(loginCallback.GetUsername()))
            {
                loginCallback.SetErrorUsername("Please enter a username");
            }

            if (String.IsNullOrEmpty(loginCallback.GetPassword()))
            {
                loginCallback.SetErrorPassword("Please enter a password");
            }

            e.Cancel = loginCallback.HasError;
        }
    }

    public void ShowDialog()
    {
        ModalDialogManager.Instance.ShowDialog();
    }
}
