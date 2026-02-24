using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class MatchmakingView : MonoBehaviourPunCallbacks
{
    [SerializeField] Text statusText;
    [SerializeField] Text RessonRoomIDText;

    [SerializeField] Text CompleteLogin;
    [SerializeField] Text WatingLogin;
    [SerializeField] Text LogOut;
    [SerializeField] Text FailedConnect;
    [SerializeField] Text Disconnected;
    public static bool roomenterdflag = false;  //生徒の入室を検知
    public BPM BPM;
    public GameController GameController;
    public EarPhoneMode EarPhoneMode;


    [SerializeField] Text LessonRoomID;
    public GameObject PoPupStudentLogOut;





    private void Start()
    {

        bool enter = false;

        RessonRoomIDText.text = LessonRoomID.text + ClassLessonRoomID.StrLessonRoomID;

        foreach (var p in PhotonNetwork.PlayerListOthers)
        {
            if (p.NickName == "Student")
            {
                enter = true;
            }
        }


        if (enter)
        {

            BPM.bpm_rpc_match();
            EarPhoneMode.RPCEarPhoneModeMatch();


            GameController.BtnChangeScaleDirectionMatch();

            roomenterdflag = true;
            PopUp(CompleteLogin);
            statusText.color = Color.red;


        }
        else
        {
            PopUp(WatingLogin);
            statusText.color = Color.white;

        }

    }





    public override void OnPlayerEnteredRoom(Player newPlayer)
    {



        BPM.bpm_rpc_match();
        EarPhoneMode.RPCEarPhoneModeMatch();


        GameController.BtnChangeScaleDirectionMatch();

        roomenterdflag = true;
        PopUp(CompleteLogin);
        statusText.color = Color.red;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.NickName == "Student")
        {
            roomenterdflag = false;
            PopUp(LogOut);
            PopUpLogOut();
                statusText.color = Color.white;



        }
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = FailedConnect.text;
       // statusText.color = Color.white;


    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        roomenterdflag = false;
        PopUp(Disconnected);

        statusText.color = Color.white;

    }



    void PopUp(Text text)
    {
        statusText.text = text.text;

    }
    void PopUpLogOut()
    {
        PoPupStudentLogOut.gameObject.SetActive(true);
    }
}