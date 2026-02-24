using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
public class End : MonoBehaviour
{
    GameObject DontDestroyPiano;
    public GameController GameController;
    // Start is called before the first frame update
    public void FinishScean()
    {

        GameController.Btn_RPCSTOPALL();
        DontDestroyPiano = GameObject.Find("DontDestroy");
        Destroy(DontDestroyPiano);
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("RoomID");


    }
}
