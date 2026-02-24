using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour
{

    // Start is called before the first frame update
    [SerializeField] GameObject PopupComponent;
    void Start()
    {


        if (PlayerPrefs.GetInt("PopupBool", 0) == 0)
        {
            PopupComponent.SetActive(true);
        }

    }
    public void KillPopup()
    {
        PopupComponent.SetActive(false);

    }
    public void Toggle()
    {
        PlayerPrefs.SetInt("PopupBool", 1);
        PlayerPrefs.Save();

    }
}
