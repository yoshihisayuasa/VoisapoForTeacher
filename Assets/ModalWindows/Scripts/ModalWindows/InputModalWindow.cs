using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InputModalWindow : ModalWindow<InputModalWindow>
{
    [SerializeField] private InputField inputField;

    private Action<string> onInputFieldDone;

    public InputModalWindow SetInputField(Action<string> onDone, string initialValue = "", string placeholderValue = "Type here")
    {
        inputField.text = initialValue;
        ((Text)inputField.placeholder).text = placeholderValue;
        onInputFieldDone = onDone;

        return this;
    }

    protected override void OnBeforeShow()
    {
        inputField.Select();
    }

    void SubmitInput()
    {
        onInputFieldDone?.Invoke(inputField.text);
        onInputFieldDone = null;
        Close();
    }

    protected override void Update()
    {
        base.Update();
        if (inputField.isFocused && inputField.text != "" && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            SubmitInput();
    }

    public void UI_InputFieldOKButton()
    {
        SubmitInput();
    }
}
