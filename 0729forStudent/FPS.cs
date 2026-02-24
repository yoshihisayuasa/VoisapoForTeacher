
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class FPS : MonoBehaviour
{

    [SerializeField] Text FPST;
    float fpsfloat;

    // Update is called once per frame
    void Update()
    {
        fpsfloat = (1f / Time.deltaTime);
        FPST.text = fpsfloat.ToString();


    }
}
