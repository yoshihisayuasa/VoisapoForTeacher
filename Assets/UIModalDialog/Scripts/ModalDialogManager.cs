using Assets.UIModalDialog.Scripts;
using System;
using UnityEngine;
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
            {
                _instance = FindObjectsByType<ModalDialogManager>(FindObjectsSortMode.None)[0];
            }
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
        Canvas[] canvasObjects = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
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

        Canvas[] canvasObjects = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        ModalDialog modalDlg = null;
        bool found = false;

        foreach (Canvas canvas in canvasObjects)
        {
            foreach (Transform transform in canvas.transform)
            {
                modalDlg = transform.GetComponent<ModalDialog>();

                if (modalDlg != null)
                {
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
            {
                break;
            }
        }

        if (modalDlg != null)
        {
            modalDlg.Show();

            DialogEventArgs dlgArgs = new DialogEventArgs();
            dlgArgs.DialogPanel = _currentDialog.transform.Find("ModalDialogPanel");
            dlgArgs.DialogName = _currentDialog.name;
            DialogOpened?.Invoke(this, dlgArgs);
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
            DialogClosingEventArgs closingArgs = new()
            {
                CloseButton = sender,
                DialogPanel = _currentDialog.transform.Find("ModalDialogPanel")
            };

            DialogClosing?.Invoke(sender, closingArgs);
            cancel = closingArgs.Cancel;
        }

        if (!cancel)
        {
            DialogClosedEventArgs closedArgs = new DialogClosedEventArgs();
            closedArgs.DialogName = _currentDialog.name;
            closedArgs.CloseButton = sender;

            _currentDialog.SetActive(false);
            _currentDialog = null;

            DialogClosed?.Invoke(sender, closedArgs);
        }
    }
}