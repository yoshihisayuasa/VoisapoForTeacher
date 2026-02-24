using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class MatchmakingView : MonoBehaviourPunCallbacks
{
    [SerializeField] private string gameVersion = "0.1";
    private string nickName = "Student";
    [SerializeField] private TMP_InputField passwordInputField = default;
    [SerializeField] private Button joinRoomButton = default;
    [SerializeField] Text btnText;
    [SerializeField] GameObject Panel;
    public GameController GameController;
    [SerializeField] Text statusText;
    [SerializeField] Text LoginLessonRoom;
    [SerializeField] Text FailedConnect;


    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = nickName;
    }


    private void Start()
    {
        joinRoomButton.interactable = false;
        passwordInputField.onValueChanged.AddListener(OnPasswordInputFieldValueChanged);
        joinRoomButton.onClick.AddListener(OnJoinRoomButtonClick);
    }



    public override void OnJoinedRoom()
    {

        joinRoomButton.interactable = true;
        btnText.text = "Logout";
        passwordInputField.interactable = false;
        Panel.gameObject.SetActive(false);

    }

    private void OnPasswordInputFieldValueChanged(string value)
    {
        // パスワードを6桁入力した時のみ、ルーム参加ボタンを押せるようにする
        value = value.Replace(" ", "").Replace("　", "");
        joinRoomButton.interactable = (value.Length == 4);
    }

    private void OnJoinRoomButtonClick()
    {
        //roomにログインする。ログイン失敗その後OnJoinRoomFailedが呼ばれる。IsConecctはFalseだからDisconnectされない。すでにConectされてるからOnconectedMasterが呼ばれずにログインできない。
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            PhotonNetwork.Disconnect();
            piano_colorinit();
        }
    }


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRoom(passwordInputField.text);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GameController.AutoScaleShift = 0;
        GameController.KeyClicked = false;
        GameController.RPCSTOPALL();
    }



    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        btnText.text = "Login";
        statusText.text = "接続に失敗しました";
        PhotonNetwork.Disconnect();
        PopUp(FailedConnect);

    }



    public override void OnDisconnected(DisconnectCause cause)
    {
        
        GameController.AutoScaleShift = 0;
        GameController.KeyClicked = false;
        btnText.text = "Login";
        passwordInputField.interactable = true;
        GameController.RPCSTOPALL();
        PopUp(LoginLessonRoom);
        piano_colorinit();


    }

    void PopUp(Text text)
    {
        Panel.gameObject.SetActive(true);
        statusText.text = text.text;

    }


    [PunRPC]
    void piano_colorinit()
    {
        for (int i = 0; i < 88; i++)
        {
            switch (GameController.Coler_Manager_init[i])
            {
                case 2:
                    if (GameController.component_cash[i] != null)
                    {
                        GameController.component_cash[i].color = Color.black;
                    }
                    break;
                case 1:
                    if (GameController.component_cash[i] != null)
                    {
                        GameController.component_cash[i].color = Color.white;
                    }
                    break;
            }
            GameController.Coler_Manager[i] = GameController.Coler_Manager_init[i];
        }
    }


    void OnApplicationFocus(bool focusStatus)
    {
        //https://greenkour.hateblo.jp/entry/2018/08/20/070000

        if (focusStatus)
        {
#if UNITY_ANDROID||UNITY_EDITOR

            AudioSettings.Reset(AudioSettings.GetConfiguration());
#endif

        }
    }

    

}
