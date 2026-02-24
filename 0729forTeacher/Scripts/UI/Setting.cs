using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Setting : MonoBehaviour
{
    GameObject DontDestroyPiano;
    // Start is called before the first frame update
    public void SettingBtn()
    {
        DontDestroyPiano = GameObject.Find("DontDestroy");
        Destroy(DontDestroyPiano);
        SceneManager.LoadScene("Menue");
    }
}
