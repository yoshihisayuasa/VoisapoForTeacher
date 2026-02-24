using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class menue : MonoBehaviour
{

    //public GameObject SettingPanel;


    public void SettingKill()
    {
        SceneManager.LoadScene("SampleScene");
        // SettingPanel.SetActive(false);
    }


    // Start is called before the first frame update
    public void Start_Menue()
    {
        SceneManager.LoadScene("RoomID");
    }



    public void PrivacyPolicy()
    {
        Application.OpenURL("https://voisapo.com/privacy");
    }

    public void TermsOfUse()
    {
        Application.OpenURL("https://voisapo.com/policy");
    }

    public void WebSite()
    {
        Application.OpenURL("https://voisapo.com/");
    }

    public void Mail()
    {
        Application.OpenURL("mailto:voisapo01@gmail.com?subject=問い合わせ");
    }

    public void UserCommunity()
    {


        Application.OpenURL("https://discord.com/invite/uTEC5tP2RF");

    }


    public void Tutorial()
    {


        Application.OpenURL("https://voisapo.com/tutorial");

    }
}
