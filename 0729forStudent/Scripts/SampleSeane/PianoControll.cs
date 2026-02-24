using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoControll : MonoBehaviour
{
    float C4posx;
    float x_;
    GameObject piano;

    void Start()
    {
        piano = GameObject.Find("Piano_object");
        C4posx = GameObject.Find("39").transform.position.x;
        x_ = 0.0f - C4posx;                       //Vector3 v3 = new Vector3(piano.transform.position.x + x_, piano.transform.position.y, piano.transform.position.z);
                                                  // piano.transform.position = v3;

        // piano.transform.position += new Vector3(x_, 0, 0);


    }


}

