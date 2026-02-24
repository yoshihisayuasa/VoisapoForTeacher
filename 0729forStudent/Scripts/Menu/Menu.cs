using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{




    // Start is called before the first frame update








    public void PrivacyPolicy()
    {
        Application.OpenURL("https://voisapo.com/privacy");
    }

    public void TermsOfUse()
    {
        Application.OpenURL("https://voisapo.com/policy");
    }

    public void VoisapoOfficial()
    {
        Application.OpenURL("https://voisapo.com");
    }

    public void ChutorialBtn()
    {
        Application.OpenURL("https://voisapo.com/tutorial");//""の中には開きたいWebページのURLを入力します
    }
}
