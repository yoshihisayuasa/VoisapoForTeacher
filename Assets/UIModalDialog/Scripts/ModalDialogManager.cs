using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;
using Assets.UIModalDialog.Scripts;
using UnityEngine.UI;

public class ModalDialogManager : MonoBehaviour
{

    public const string MODAL_DLG_TAG = "ModalDialog";

    public event EventHandler<DialogEventArgs> DialogOpened;

    public event EventHandler<DialogClosingEventArgs> DialogClosing;

    public event EventHandler<DialogClosedEventArgs> DialogClosed;

    private GameObject _currentDialog = null;

    #region Create Singleton

    private static ModalDialogManager _instance;

    public static ModalDialogManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<ModalDialogManager>();
            return _instance;
        }
    }

    #endregion

    void Awake()
    {
        HideAllDialogs();
    }

    /// <summary>
    /// Hide all dialogs
    /// </summary>
    private void HideAllDialogs()
    {
        Canvas[] canvasObjects = GameObject.FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvasObjects)
        {
            foreach (Transform transform in canvas.transform)
            {
                ModalDialog modalDlg = transform.GetComponent<ModalDialog>();
                if (modalDlg != null)
                {
                    modalDlg.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Gets the current dialog name
    /// </summary>
    public GameObject CurrentDialog
    {
        get { return _currentDialog; }
    }

    /// <summary>
    /// Shows the first modal dialog that can be found in all Canvas objects
    /// </summary>
    public void ShowDialog(string dialogName = "")
    {
        if (_currentDialog != null)
        {
            Debug.LogError("A modal dialog is already opened");
            return;
        }

        // Find Canvas
        Canvas[] canvasObjects = GameObject.FindObjectsOfType<Canvas>();

        ModalDialog modalDlg = null;

        bool found = false;

        // Get the first modal dialog and open it
        foreach (Canvas canvas in canvasObjects)
        {
            foreach (Transform transform in canvas.transform)
            {
                modalDlg = transform.GetComponent<ModalDialog>();

                if (modalDlg != null)
                {
                    // Try to find a special dialog
                    if (!String.IsNullOrEmpty(dialogName))
                    {
                        if (transform.name == dialogName)
                        {
                            found = true;
                        }
                    }
                    else
                    {
                        found = true;
                    }

                    if (found)
                    {
                        _currentDialog = transform.gameObject;
                        break;
                    }
                }
            }

            if (found)
                break;
        }

        if (modalDlg != null)
        {
            modalDlg.Show();

            if (DialogOpened != null)
            {
                DialogEventArgs dlgArgs = new DialogEventArgs();
                dlgArgs.DialogPanel = _currentDialog.transform.Find("ModalDialogPanel");
                dlgArgs.DialogName = _currentDialog.name;

                DialogOpened(this, dlgArgs);
            }
        }
        else
        {
            Debug.LogError("No modal dialog found to show");
        }

    }

    public void CloseDialog(Button sender)
    {
        bool cancel = false;

        if (DialogClosing != null)
        {
            DialogClosingEventArgs dlgArgs = new DialogClosingEventArgs();
            dlgArgs.CloseButton = sender;
            dlgArgs.DialogPanel = _currentDialog.transform.Find("ModalDialogPanel");

            DialogClosing(sender, dlgArgs);

            cancel = dlgArgs.Cancel;
        }

        if (!cancel)
        {
            DialogClosedEventArgs dlgArgs = new DialogClosedEventArgs();
            dlgArgs.DialogName = _currentDialog.name;
            dlgArgs.CloseButton = sender;

            _currentDialog.SetActive(false);
            _currentDialog = null;

            if (DialogClosed != null)
            {
                DialogClosed(sender, dlgArgs);    
            }
        }

    }
}
