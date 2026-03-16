using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoginCallback : MonoBehaviour {

    private GameObject _errorPanelUsername;

    private GameObject _errorPanelPassword;

    private GameObject _inputUsername;

    private GameObject _inputPassword;

    private bool _hasError = false;

    void Awake()
    {
        _errorPanelUsername = transform.Find("ErrorPanelUsername").gameObject;

        _errorPanelPassword = transform.Find("ErrorPanelPassword").gameObject;

        _inputUsername = transform.Find("InputUsername").gameObject;

        _inputPassword = transform.Find("InputPassword").gameObject;
    }

    public void ClearInput()
    {
        _inputUsername.GetComponent<InputField>().text = "";
        _inputPassword.GetComponent<InputField>().text = "";
    }

    public void SetErrorUsername(string text)
    {
        SetError(_errorPanelUsername, text);
    }

    public void SetErrorPassword(string text)
    {
        SetError(_errorPanelPassword, text);
    }

    public String GetUsername()
    {
        return _inputUsername.GetComponent<InputField>().text;
    }

    public String GetPassword()
    {
        return _inputPassword.GetComponent<InputField>().text;
    }

    public void ClearErrors()
    {
        SetError(_errorPanelUsername, "");
        SetError(_errorPanelPassword, "");

        _hasError = false;
    }

    public bool HasError
    {
        get { return _hasError; }
    }

    protected void SetError(GameObject errorPanel, string errorText)
    {
        if (String.IsNullOrEmpty(errorText))
        {
            errorPanel.SetActive(false);
        }
        else
        {
            errorPanel.SetActive(true);
            errorPanel.transform.Find("Text").GetComponent<Text>().text = errorText;
            _hasError = true;
        }
    }
	
}
