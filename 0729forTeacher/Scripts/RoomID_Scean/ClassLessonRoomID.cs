using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ClassLessonRoomID : MonoBehaviourPunCallbacks
{
    public static string StrLessonRoomID = "----";
    // Start is called before the first frame update
    // private Dictionary<string, RoomInfo> dictionary = new Dictionary<string, RoomInfo>();
    // private byte maxPlayers = 2;
    private string nickName = "Trainer";
    [SerializeField]
    private string gameVersion = "0.1";

    public static Text TxtLessonRoomID;


    void Start()
    {

        //ネットワーク変数初期化
        GameController.AutoScaleShift = 0;
        EarPhoneMode.EarPhoneModeBool = false;
        BPM.bpm = 180;
        MatchmakingView.roomenterdflag = false;



        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = nickName;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }






        TxtLessonRoomID = GameObject.Find("TextLessonID").GetComponent<Text>();

    }


    void Update()
    {



        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();

        }



    }

    public override void OnConnectedToMaster()
    {
        PublishID();
    }


    public void StartBtn()
    {
        SceneManager.LoadScene("SampleScene");
    }



    public void PublishID()
    {

        StrLessonRoomID = Random.Range(1000, 9999).ToString();
        TxtLessonRoomID.text = StrLessonRoomID;
        PhotonNetwork.CreateRoom(StrLessonRoomID);


    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        PublishID();


    }



}




