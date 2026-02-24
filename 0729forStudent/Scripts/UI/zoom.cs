using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zoom : MonoBehaviour
{
    [SerializeField]
    GameObject piano;


    // Start is called before the first frame update
    void Awake()
    {
        piano.transform.localScale = new Vector3(PlayerPrefs.GetFloat("PianoScale_Android", 0.9f), 1, 1);
    }
    public void positive()
    {
        piano.transform.localScale += new Vector3(0.05f, 0, 0);
        PlayerPrefs.SetFloat("PianoScale_Android", piano.transform.localScale.x);
        PlayerPrefs.Save();

    }
    public void negative()
    {
        if (piano.transform.localScale.x > 0.2f) //反転防止
        {
            piano.transform.localScale += new Vector3(-0.05f, 0, 0);
            PlayerPrefs.SetFloat("PianoScale_Android", piano.transform.localScale.x);
            PlayerPrefs.Save();
        }

    }
}
