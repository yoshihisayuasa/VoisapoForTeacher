using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class keyevent : MonoBehaviour
{
    [SerializeField] Button BPM_Positive;
    [SerializeField] Button BPM_Negative;

    [SerializeField] Button Cancel;
    [SerializeField] Button BtnAutscaleUp;
    [SerializeField] Button BtnAutscaleDwn;
    public static bool shiftflag = false;
    // Start is called before the first frame update

    // Update is called once per frame

    void Update()
    {

        if (Input.anyKeyDown && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {

            keycontroll();
        }


    }


    //http://fantom1x.blog130.fc2.com/blog-entry-326.html?sp
    private void keycontroll()
    {

        if (Input.GetKeyDown(KeyCode.E) == true || Input.GetKeyDown(KeyCode.U) == true)
        {
            BPM_Negative.onClick.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.R) == true || Input.GetKeyDown(KeyCode.I) == true)
        {
            BPM_Positive.onClick.Invoke();
        }
        else if (Input.GetKey(KeyCode.Space) == true)
        {
            Cancel.onClick.Invoke();
        }

        else if (Input.GetKeyDown(KeyCode.C) == true || Input.GetKeyDown(KeyCode.Comma) == true)
        {
            BtnAutscaleUp.onClick.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X) == true || Input.GetKeyDown(KeyCode.M) == true)
        {
            BtnAutscaleDwn.onClick.Invoke();
        }

    }
}
