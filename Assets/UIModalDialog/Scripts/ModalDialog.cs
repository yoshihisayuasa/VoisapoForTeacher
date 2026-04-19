using UnityEngine;
using UnityEngine.UI;

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

        if (_submitButtons == null)
        {
            return;
        }

        // Bind button
        foreach (Button button in _submitButtons)
        {
            BindButton(button);
        }
    }

    private void BindButton(Button button)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => Close(button));
    }

    public void Close(Button button)
    {
        ModalDialogManager.Instance.CloseDialog(button);
    }
}