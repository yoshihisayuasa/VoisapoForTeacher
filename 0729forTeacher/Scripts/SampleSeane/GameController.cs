
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class GameController : MonoBehaviourPunCallbacks
{

    AudioSource sounds;
    public static Camera camera_object;
    GameObject OnMouseObject;
    String NewOnMouseObjectName = "36";
    string[] array_keyname = new string[88];
    string key_name = "36";

    static GameObject piano;
    GameObject[] object2 = new GameObject[88];
    public static SpriteRenderer[] component_cash = new SpriteRenderer[88];
    // public static Image[] component_cash = new Image[88];

    static int index_onmouse = 36;
    public static int[] array_scale1 = new int[110];  //0 スケールのスタートの音。1,最低音,2最高音。10～60 スケール.60～110、拍

    public static int arr_scale_Length; //デフォルトで表示するスケールの数


    public static int[] Coler_Manager_init = new int[88] { 1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,
                                            1,2,1,2,1,1,2,1,2,1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,1};
    //white=1,black=2

    public static int[] array_dammy = new int[0];
    public static int[] Coler_Manager = new int[88]; //white=1 black=2 red=3 pink=4 darkpink=5
    // public static string[] Coler_Manager_BlueYellow = new string[88];
    public static int[] Coler_Manager_BlueYellow = new int[88];//white=1 black=2 darkblue=3,lightblue=4,darkyellow=5,yellow=6  darkblue=1,lightblue=2,darkyellow=3,yellow=4


    public static int array_scale_max = 0;
    public static int array_scale_min = 0;

    static int max_onmouse = 100;

    static int min_onmouse = 0;
    static Transform[] object2_transform = new Transform[88];
    [SerializeField] Text trainerside;
    [SerializeField] Text studentside;
    AudioClip[] clip = new AudioClip[88];
    AudioClip metronome;
    static Color darkpink = new Color(0.675f, 0.463f, 0.451f, 1.0f);
    static Color pink = new Color(1, 0.5f, 0.6f, 1);
    static Color lightblue = new Color(0, 0.682f, 0.937f, 1.0f);
    static Color darkblue = new Color(0.0f, 0.364f, 0.50f, 1.0f);
    static Color darkyellow = new Color(0.50f, 0.50f, 0.0f, 1.0f);
    public static bool ScaleSelect = false;
    public static int AutoScaleShift = 0; //staticだと終了時色が残る。レッスンルームID発行画面で初期化。マッチしたときに同期
    bool InCord = false;

    int Playside = 0;//1が生徒。2がトレーナー。直前がどちら側の再生だったか把握
    bool LastScale = true;

    bool ChangeScaleDirection;

    public GameObject BtnAutscaleDwn;
    public GameObject BtnAutscaleUp;
    Image BtnAutscaleDwnSP;
    Image BtnAutscaleUpSP;
    bool BoolTrainerDemo = false;
    bool KeyClicked = false;
    bool sending = false;
    [SerializeField] GameObject PurchaseRequestBord;

    bool BoolControl;


    [SerializeField] GameObject KeyA0;
    [SerializeField] GameObject KeyC8;
    static Transform TransformKeyA0;
    static Transform TransformKeyC8;
    static Transform PianoTransform;

    static float camewrld_max = 0;
    static float camewrld_min = 0;
    static Vector3 max_keypos;
    static Vector3 min_keypos;
    static Vector3 V3;
    static float middle;


    //通信に使う変数は全てstaticで宣言。レッスンルームID発行画面で初期化。MatchMakingViewのOnEnterdRoomで同期。生徒アプリでログアウトしたときに初期化。


    void Start()
    {

        Application.targetFrameRate = 60;
        piano = GameObject.Find("Piano_object");
        camera_object = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        sounds = GetComponent<AudioSource>();


        BtnAutscaleUpSP = BtnAutscaleUp.GetComponent<Image>();
        BtnAutscaleDwnSP = BtnAutscaleDwn.GetComponent<Image>();



        if (AutoScaleShift == -1)
        {
            BtnAutscaleDwnSP.color = Color.red;
        }
        if (AutoScaleShift == 1)
        {
            BtnAutscaleUpSP.color = Color.red;
        }




        for (int i = 0; i < 88; i++)
        {
            object2[i] = GameObject.Find(i.ToString());
            object2_transform[i] = object2[i].transform;
            component_cash[i] = object2[i].GetComponent<SpriteRenderer>();
            array_keyname[i] = object2[i].name;
            Coler_Manager[i] = Coler_Manager_init[i];
            clip[i] = Resources.Load("ogg/" + i, typeof(AudioClip)) as AudioClip;
        }
        metronome = Resources.Load("metronome/metronome", typeof(AudioClip)) as AudioClip;



        max_onmouse = array_scale_max + index_onmouse;
        if (max_onmouse < 88)
        {
            component_cash[max_onmouse].color = lightblue;
        }
        min_onmouse = array_scale_min + index_onmouse;
        if ((min_onmouse > -1) && (max_onmouse < 88))
        {
            component_cash[min_onmouse].color = lightblue;
        }

        component_cash[index_onmouse].color = Color.yellow;

        PianoTransform = piano.transform;
        TransformKeyA0 = KeyA0.transform;
        TransformKeyC8 = KeyC8.transform;

        V3 = new Vector3(PianoTransform.position.x, PianoTransform.position.y, PianoTransform.position.z);



    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            BoolControl = true;
        }

#if UNITY_STANDALONE_OSX
        else if (Input.GetKey(KeyCode.LeftApple) || Input.GetKey(KeyCode.RightApple))
        {

            BoolControl = true;
        }
#endif

        else
        {
            BoolControl = false;
        }


        if (BoolControl)
        {
            if (!BoolTrainerDemo)
            {
                if (MatchmakingView.roomenterdflag)
                {
                    if (!sending)
                    {
                        photonView.RPC(nameof(RPCPushControl), RpcTarget.Others, true);
                        sending = true;
                    }
                }
                else
                {
                    BoolTrainerDemo = true;
                }

            }
        }
        else
        {

            if (BoolTrainerDemo)
            {

                if (MatchmakingView.roomenterdflag)
                {
                    photonView.RPC(nameof(RPCPushControl), RpcTarget.All, false);

                }
                else
                {
                    BoolTrainerDemo = false;

                }

            }

        }


        if (((AutoScaleShift != 0 && LastScale == true) || ChangeScaleDirection && InCord) && KeyClicked)
        {
            if (MatchmakingView.roomenterdflag)
            {

                if (BoolTrainerDemo)
                {
                    photonView.RPC(nameof(all_clrst), RpcTarget.All, array_dammy);

                    if (Playside == 2) //直前が先生発声だったら配列更新
                    {
                        AutoscaleShiftArray();
                    }
                    play(1);
                }

            }


            else
            {
                all_clrst(array_dammy);

                if (BoolControl)
                {
                    if (Playside == 2)
                    {
                        AutoscaleShiftArray();
                    }
                    play(3);  //オラインモードでトレーナー発声

                }
                else
                {
                    if (Playside == 1) //直前トレナー再生の場合は更新しない
                    {
                        AutoscaleShiftArray();
                    }
                    play(2);//オフラインモードで生徒発声
                }
            }



        }

        if (ChangeScaleDirection == true)
        {
            ChangeScaleDirection = false;
        }


        if (Input.GetKeyDown(KeyCode.Return) == true || Input.GetKeyDown(KeyCode.D) == true || Input.GetKeyDown(KeyCode.K) == true)
        {
            Selected();
        }


        Ray ray = camera_object.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit2d = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction);



        if (ScaleSelect)
        {
            ScaleSelect = false;

            swich_color(max_onmouse);
            swich_color(min_onmouse);
            swich_color(index_onmouse);

            NewOnMouseObjectName = key_name;
            index_onmouse = int.Parse(NewOnMouseObjectName);


            max_onmouse = array_scale_max + index_onmouse;
            min_onmouse = array_scale_min + index_onmouse;

            Color_Blue_Yellow();


        }


        if (Input.GetKeyDown(KeyCode.S) == true || Input.GetKeyDown(KeyCode.J) == true)
        {
            kydwn_nxtscle_dwn();
        }
        else if (Input.GetKeyDown(KeyCode.F) == true || Input.GetKeyDown(KeyCode.L) == true)
        {
            kydwn_nxtscle_up();
        }




        if (EventSystem.current.IsPointerOverGameObject()) return; //レイキャスト透過防止

        if (hit2d)
        {

            OnMouseObject = hit2d.transform.gameObject;

            if (OnMouseObject.CompareTag("key"))
            {
                key_name = OnMouseObject.name;

                if ((key_name != NewOnMouseObjectName) && (NewOnMouseObjectName != null))
                {


                    swich_color(max_onmouse);
                    swich_color(min_onmouse);
                    swich_color(index_onmouse);


                    NewOnMouseObjectName = key_name;
                    index_onmouse = int.Parse(NewOnMouseObjectName);


                    max_onmouse = array_scale_max + index_onmouse;
                    min_onmouse = array_scale_min + index_onmouse;

                    Color_Blue_Yellow();



                }

                if (Input.GetMouseButtonDown(0))
                {
                    Selected();

                }

            }


        }
    }

    [PunRPC]
    void RPCPLAY(int[] array_scale1)
    {
        StartCoroutine("IEPLAY", array_scale1);
    }


    [PunRPC]
    public void RPCSTOPALL()
    {
        StopAllCoroutines();

        trainerside.color = Color.white;
        studentside.color = Color.white;
        sounds.Stop();

    }


    public void Btn_RPCSTOPALL()
    {
        if (MatchmakingView.roomenterdflag)
        {
            photonView.RPC(nameof(RPCSTOPALL), RpcTarget.All);
            photonView.RPC(nameof(RPCKeyClicked), RpcTarget.All, false);


        }
        else
        {
            sounds.Stop();
            StopAllCoroutines();
            RPCKeyClicked(false);
        }



    }


    [PunRPC]
    private void RPCCOLER(int[] array_scale1)
    {
        StartCoroutine("IECOLER", array_scale1);
    }

    [PunRPC]
    private void RPC_IEPLAYSIDE_Trainer(int[] array_scale1)
    {
        StartCoroutine("IEPLAYSIDE_Trainer", array_scale1);
    }
    [PunRPC]
    private void RPC_IEPLAYSIDE_Studenet(int[] array_scale1)
    {
        StartCoroutine("IEPLAYSIDE_Student", array_scale1);
    }




    private IEnumerator IEPLAY(int[] array_scale2)
    {

        if (MatchmakingView.roomenterdflag)
        {
            photonView.RPC(nameof(RPCLasetScale), RpcTarget.All, false);

        }
        else
        {
            RPCLasetScale(false);
        }


        sounds.Stop();

        for (int i = 0; i < 50; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }
            if ((!(array_scale1[i + 10] + array_scale1[0] < 88)) || !(array_scale1[i + 10] + array_scale1[0] > -1))
            {
                goto END;
            }
        }



        if (AutoScaleShift != 0)
        {
            InCord = true;
        }

        for (int i = 0; i < 3; i++)  //和音を再生
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }

            sounds.PlayOneShot(clip[array_scale1[0] + array_scale1[10 + i]], volcontroll.newSliderValue);
        }

        for (int i = 0; i < array_scale1[60]; i++)  //メトロノーム再生
        {
            sounds.PlayOneShot(metronome, volcontroll.newSliderValue);
            yield return WaitForSecondsCache.Wait(BPM.bpm_time());

        }

        InCord = false;



        for (int i = 3; i < 50; i++)
        {

            sounds.Stop();
            if (array_scale1[60 + i] == 0)
            {
                break;
            }

            sounds.PlayOneShot(clip[array_scale1[0] + array_scale1[10 + i]], volcontroll.newSliderValue);
            yield return WaitForSecondsCache.Wait(BPM.bpm_time() * array_scale1[60 + i]);



        }

        if (MatchmakingView.roomenterdflag)
        {
            photonView.RPC(nameof(RPCLasetScale), RpcTarget.All, true);
        }
        else
        {

            LastScale = true;
        }

    END:;



    }

    [PunRPC]
    private void RPCIEPLAY_Waon(int[] array_scale2)
    {
        StartCoroutine("IEPLAY_Waon", array_scale2);
    }


    private IEnumerator IEPLAY_Waon(int[] array_scale2)
    {


        sounds.Stop();

        for (int i = 0; i < 50; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }
            if ((!(array_scale1[i + 10] + array_scale1[0] < 88)) || !(array_scale1[i + 10] + array_scale1[0] > -1))
            {
                goto END;
            }
        }


        for (int i = 0; i < 3; i++)  //和音を再生
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }
            sounds.PlayOneShot(clip[array_scale1[0] + array_scale1[10 + i]], volcontroll.newSliderValue);

        }

        for (int i = 0; i < 2; i++)
        {

            yield return WaitForSecondsCache.Wait(BPM.bpm_time());

        }

        /*
                for (int i = 0; i < 3; i++)  //和音を再生
                {
                    if (array_scale1[60 + i] == 0)
                    {
                        break;
                    }
                    sounds.PlayOneShot(clip[array_scale1[0] + array_scale1[10 + i]], volcontroll.newSliderValue);

                }
                /*
                for (int i = 0; i < array_scale1[60]; i++)  //メトロノーム再生
                {
                    sounds.PlayOneShot(metronome, volcontroll.newSliderValue);
                    yield return WaitForSecondsCache.Wait(BPM.bpm_time());

                }
                */

        sounds.Stop();

    END:;


    }





    private IEnumerator IEPLAYSIDE_Trainer(int[] array_scale2)
    {
        int time = array_scale1[60]; //和音の拍数は60番目に入っている


        for (int i = 0; i < 50; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }
            if ((!(array_scale1[i + 10] + array_scale1[0] < 88)) && (array_scale1[i + 10] + array_scale1[0] > -1))
            {
                goto END;
            }
        }

        trainerside.color = Color.red;

        for (int i = 3; i < 50; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }
            time = time + array_scale1[60 + i];

        }
        yield return WaitForSecondsCache.Wait(BPM.bpm_time() * time);
        trainerside.color = Color.white;

    END:;

    }

    private IEnumerator IEPLAYSIDE_Student(int[] array_scale2)
    {
        int time = array_scale1[60]; //和音の拍数は60番目に入っている


        for (int i = 0; i < 50; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }
            if ((!(array_scale1[i + 10] + array_scale1[0] < 88)) && (array_scale1[i + 10] + array_scale1[0] > -1))
            {
                goto END;
            }
        }

        studentside.color = Color.blue;

        for (int i = 3; i < 50; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }
            time = time + array_scale1[60 + i];

        }
        yield return WaitForSecondsCache.Wait(BPM.bpm_time() * time);
        studentside.color = Color.white;
    END:;

    }




    private IEnumerator IECOLER(int[] array_scale2)
    {
        array_scale1 = array_scale2;


        for (int i = 0; i < 50; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }
            if (!((array_scale1[i + 10] + array_scale1[0] < 88) && (array_scale1[i + 10] + array_scale1[0] > -1)))
            {
                goto END;
            }
        }



        for (int i = 0; i < 3; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }

            component_cash[array_scale1[i + 10] + array_scale1[0]].color = Color.red;
            Coler_Manager[array_scale1[i + 10] + array_scale1[0]] = 3;


        }
        yield return WaitForSecondsCache.Wait(BPM.bpm_time() * array_scale1[60]);




        for (int i = 0; i < 3; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }

            if (Coler_Manager_init[array_scale1[i + 10] + array_scale1[0]] == 2)
            {
                component_cash[array_scale1[i + 10] + array_scale1[0]].color = Color.black;
                Coler_Manager[array_scale1[i + 10] + array_scale1[0]] = 2;
            }

            else
            {
                component_cash[array_scale1[i + 10] + array_scale1[0]].color = Color.white;
                Coler_Manager[array_scale1[i + 10] + array_scale1[0]] = 1;
            }
        }


        for (int i = 3; i < 50; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }

            component_cash[array_scale1[i + 10] + array_scale1[0]].color = Color.red;
            Coler_Manager[array_scale1[i + 10] + array_scale1[0]] = 3;



            yield return WaitForSecondsCache.Wait(BPM.bpm_time() * array_scale1[60 + i]);


            if (Coler_Manager_init[array_scale1[i + 10] + array_scale1[0]] == 2)
            {
                component_cash[array_scale1[i + 10] + array_scale1[0]].color = darkpink;
                Coler_Manager[array_scale1[i + 10] + array_scale1[0]] = 5;
            }

            else
            {
                component_cash[array_scale1[i + 10] + array_scale1[0]].color = pink;
                Coler_Manager[array_scale1[i + 10] + array_scale1[0]] = 4;
            }
        }

    END:;

    }


    [PunRPC]
    private void all_clrst(int[] array_scale2)
    {
        for (int i = 0; i < 50; i++)
        {
            if (i > 3) //和音はブレイク判定の対象外
            {
                if (array_scale1[60 + i] == 0)
                {
                    break;
                }
            }

            if ((array_scale1[i + 10] + array_scale1[0] < 88) && (array_scale1[i + 10] + array_scale1[0] > -1))
            {
                switch (Coler_Manager_init[array_scale1[i + 10] + array_scale1[0]])
                {
                    case 2:
                        component_cash[array_scale1[i + 10] + array_scale1[0]].color = Color.black;
                        Coler_Manager[array_scale1[i + 10] + array_scale1[0]] = 2;
                        break;
                    case 1:
                        component_cash[array_scale1[i + 10] + array_scale1[0]].color = Color.white;
                        Coler_Manager[array_scale1[i + 10] + array_scale1[0]] = 1;
                        break;
                }
            }
        }



    }





    private void play(int playmode)
    {

        switch (playmode)
        {

            case 0:  //生徒側で再生
                photonView.RPC(nameof(RPCSTOPALL), RpcTarget.All);
                photonView.RPC(nameof(RPCCOLER), RpcTarget.All, array_scale1);
                photonView.RPC(nameof(RPC_IEPLAYSIDE_Studenet), RpcTarget.All, array_scale1);
                photonView.RPC(nameof(RPCPlayside1), RpcTarget.All, 1);
                photonView.RPC(nameof(RPCPLAY), RpcTarget.Others, array_scale1);


                if (EarPhoneMode.EarPhoneModeBool)
                {
                    StartCoroutine("IEPLAY_Waon", array_scale1);
                }

                break;

            case 1://先生側で再生デモ
                photonView.RPC(nameof(RPCPlayside1), RpcTarget.All, 2);
                photonView.RPC(nameof(RPCSTOPALL), RpcTarget.All);
                RPCPLAY(array_scale1);
                photonView.RPC(nameof(RPCCOLER), RpcTarget.All, array_scale1);
                photonView.RPC(nameof(RPC_IEPLAYSIDE_Trainer), RpcTarget.All, array_scale1);

                break;

            case 2://オフラインモード生徒発声

                Playside = 1;

                RPCSTOPALL();
                RPCPLAY(array_scale1);
                RPCCOLER(array_scale1);
                RPC_IEPLAYSIDE_Studenet(array_scale1);

                break;

            case 3://オフラインモードで先生デモ
                Playside = 2;
                RPCSTOPALL();
                RPCPLAY(array_scale1);
                RPCCOLER(array_scale1);
                RPC_IEPLAYSIDE_Trainer(array_scale1);

                break;

        }
    }




    static void piano_automove(int scale_min, int scale_max)
    {



        if (scale_max < 88 && scale_min > -1 && scale_min < 88 && scale_max > -1)
        {


            max_keypos = object2_transform[scale_max].position;
            min_keypos = object2_transform[scale_min].position;

            camewrld_max = camera_object.WorldToViewportPoint(max_keypos).x;
            camewrld_min = camera_object.WorldToViewportPoint(min_keypos).x;


            if (camewrld_max > 1 || (camewrld_min < 0))
            {
                middle = (max_keypos.x + min_keypos.x) / 2;


                V3.x = PianoTransform.position.x - middle;
                PianoTransform.position = V3;


                if (TransformKeyA0.position.x > PianoControll.dwnlimmit)
                {

                    V3.x -= (TransformKeyA0.position.x - PianoControll.dwnlimmit);


                    PianoTransform.position = V3;
                }
                else if (TransformKeyC8.position.x < PianoControll.uplimmit)
                {
                    V3.x += (PianoControll.uplimmit - TransformKeyC8.position.x);

                    PianoTransform.position = V3;
                }


            }
        }
    }

    public void kydwn_nxtscle_dwn()
    {

        if (min_onmouse > 5)
        {
            if (Input.GetKey(KeyCode.LeftShift) == true || (Input.GetKey(KeyCode.RightShift) == true))
            {
                swich_color(index_onmouse);
                swich_color(min_onmouse);
                swich_color(max_onmouse);
                index_onmouse = index_onmouse - 6;

                all_clrst(array_dammy);
                max_onmouse = array_scale_max + index_onmouse;
                min_onmouse = array_scale_min + index_onmouse;

                if ((min_onmouse > -1) && (max_onmouse < 88))
                {
                    piano_automove(min_onmouse, max_onmouse);
                }
                Color_Blue_Yellow();


            }
        }

        if (Input.GetKey(KeyCode.LeftShift) == false && (Input.GetKey(KeyCode.RightShift) == false) && (min_onmouse > 0))
        {

            swich_color(index_onmouse);
            swich_color(min_onmouse);
            swich_color(max_onmouse);
            index_onmouse = index_onmouse - 1;

            all_clrst(array_dammy);
            max_onmouse = array_scale_max + index_onmouse;
            min_onmouse = array_scale_min + index_onmouse;

            if ((min_onmouse > -1) && (max_onmouse < 88))
            {
                piano_automove(min_onmouse, max_onmouse);
            }
            Color_Blue_Yellow();

        }




    }


    public void kydwn_nxtscle_up()
    {

        if (max_onmouse < 82)
        {
            if (Input.GetKey(KeyCode.LeftShift) == true || (Input.GetKey(KeyCode.RightShift) == true))
            {
                swich_color(index_onmouse);
                swich_color(min_onmouse);
                swich_color(max_onmouse);

                index_onmouse = index_onmouse + 6;

                all_clrst(array_dammy);
                max_onmouse = array_scale_max + index_onmouse;
                min_onmouse = array_scale_min + index_onmouse;

                if ((min_onmouse > -1) && (max_onmouse < 88))
                {
                    piano_automove(min_onmouse, max_onmouse);
                }
                Color_Blue_Yellow();


            }
        }

        if (Input.GetKey(KeyCode.LeftShift) == false && (Input.GetKey(KeyCode.RightShift) == false) && (max_onmouse < 87))
        {
            swich_color(index_onmouse);
            swich_color(min_onmouse);
            swich_color(max_onmouse);

            index_onmouse = index_onmouse + 1;

            all_clrst(array_dammy);
            max_onmouse = array_scale_max + index_onmouse;
            min_onmouse = array_scale_min + index_onmouse;

            if ((min_onmouse > -1) && (max_onmouse < 88))
            {
                piano_automove(min_onmouse, max_onmouse);
            }
            Color_Blue_Yellow();

        }

    }

    public static void Get_ArrayScale1()
    {

        for (int i = 10; i < 110; i++)
        {
            array_scale1[i] = 0;
        }

        for (int i = 0; i < arr_scale_Length; i++)
        {
            array_scale1[i + 10] = ControlScale.arr_scale[i, 0];
            array_scale1[i + 60] = ControlScale.arr_scale[i, 1];
        }
        array_scale1[1] = array_scale_max;
        array_scale1[2] = array_scale_min;
    }



    static void swich_color(int number)
    {

        if (number < 88 && number > -1)
        {
            switch (Coler_Manager[number])
            {


                case 2:
                    component_cash[number].color = Color.black;
                    break;
                case 1:
                    component_cash[number].color = Color.white;
                    break;

                case 3:
                    component_cash[number].color = Color.red;
                    break;
                case 4:
                    component_cash[number].color = pink;
                    break;
                case 5:
                    component_cash[number].color = darkpink;
                    break;

            }
        }

    }

    [PunRPC]
    private void piano_colorinit()
    {
        for (int i = 0; i < 88; i++)
        {
            switch (Coler_Manager_init[i])
            {

                case 2:
                    if (component_cash[i] != null)
                    {
                        component_cash[i].color = Color.black;
                    }
                    break;
                case 1:
                    if (component_cash[i] != null)
                    {
                        component_cash[i].color = Color.white;
                    }
                    break;
            }
            Coler_Manager[i] = Coler_Manager_init[i];
        }
    }

    private static void Color_Blue_Yellow()
    {

        if ((min_onmouse > -1) && (max_onmouse < 88))
        {

            if (Coler_Manager_init[min_onmouse] == 2)
            {
                component_cash[min_onmouse].color = darkblue;
                Coler_Manager_BlueYellow[min_onmouse] = 3;
            }
            else
            {

                component_cash[min_onmouse].color = lightblue;
                Coler_Manager_BlueYellow[min_onmouse] = 4;

            }

            if (Coler_Manager_init[max_onmouse] == 2)
            {
                component_cash[max_onmouse].color = darkblue;
                Coler_Manager_BlueYellow[min_onmouse] = 3;
            }

            else
            {
                component_cash[max_onmouse].color = lightblue;
                Coler_Manager_BlueYellow[min_onmouse] = 4;
            }



            if (Coler_Manager_init[index_onmouse] == 2)
            {
                component_cash[index_onmouse].color = darkyellow;
            }
            else
            {
                component_cash[index_onmouse].color = Color.yellow;

            }
        }
    }



    [PunRPC]
    void RPCLasetScale(bool LastScale1)
    {
        LastScale = LastScale1;

    }
    [PunRPC]
    void RPCInCord(bool InCordBol)
    {
        InCord = InCordBol;

    }

    public void BtnChangeScaleDirection(int UpDwn)
    {

        if (PlayerPrefs.GetInt("subsc", 0) == 1)
        // if (true)
        {


            if (AutoScaleShift != UpDwn)
            {
                if (AutoScaleShift != 0)
                {
                    ChangeScaleDirection = true;
                }
                AutoScaleShift = UpDwn;

                if (AutoScaleShift == -1)
                {
                    BtnAutscaleDwnSP.color = Color.red;
                    BtnAutscaleUpSP.color = Color.white;

                }
                if (AutoScaleShift == 1)
                {
                    BtnAutscaleUpSP.color = Color.red;
                    BtnAutscaleDwnSP.color = Color.white;
                }


                if (!LastScale)
                {

                    FuncKeyClicked(true);
                }


            }
            else
            {


                AutoScaleShift = 0;
                ChangeScaleDirection = false;

                BtnAutscaleUpSP.color = Color.white;
                BtnAutscaleDwnSP.color = Color.white;
                FuncKeyClicked(false);
            }

            if (MatchmakingView.roomenterdflag)
            {
                photonView.RPC(nameof(RPCBtnChangeScaleDirection), RpcTarget.All, AutoScaleShift, ChangeScaleDirection);
            }
        }
        else
        {

            ClickPurchaseObject();
        }

    }


    public void BtnChangeScaleDirectionMatch()
    {

        photonView.RPC(nameof(RPCBtnChangeScaleDirection), RpcTarget.Others, AutoScaleShift, ChangeScaleDirection);

    }



    [PunRPC]
    public void RPCBtnChangeScaleDirection(int UpDwn, bool ChangeScaleDirection)
    {

    }


    [PunRPC]
    public void RPCPushControl(bool TrueFalse)
    {
        BoolTrainerDemo = TrueFalse;
    }

    [PunRPC]
    public void RPCPushControlRecive(bool TrueFalse)
    {
        BoolTrainerDemo = TrueFalse;
        sending = false;
    }

    [PunRPC]
    public void RPCPlayside1(int PlaysideNumber)
    {
        Playside = PlaysideNumber;
    }




    public void FuncKeyClicked(bool TrueFalse)//自動移調中にキーをクリックしたかを判定する機能
    {
        //これがないと自動移調機能をおんにしていないときにキーをクリックして、その後移調をクリックすると勝手にキーが進んでしまう
        if (!TrueFalse)
        {
            KeyClicked = false;
        }
        else
        {
            if (AutoScaleShift != 0)
            {
                KeyClicked = true;
            }
            else
            {
                KeyClicked = false;
            }
        }

        if (MatchmakingView.roomenterdflag)
        {
            photonView.RPC(nameof(RPCKeyClicked), RpcTarget.Others, KeyClicked);
        }

    }

    [PunRPC]
    public void RPCKeyClicked(bool TrueFalse)
    {

    }




    public void ClickPurchaseObject()
    {

        if (PurchaseRequestBord != null)
        {
            if (!PurchaseRequestBord.activeSelf)
            {
                PurchaseRequestBord.SetActive(true);
            }
        }

    }



    public void Create()
    {


        Btn_RPCSTOPALL();

        if ((PlayerPrefs.GetInt("subsc", 0) == 1))
        // if (true)
        {
            if ((ControlScale.scl_num < 26))
            {
                SceneManager.LoadScene("CreateScale");

                for (int i = 0; i < 88; i++)
                {
                    switch (GameController.Coler_Manager_init[i])
                    {
                        case 2:
                            GameController.component_cash[i].color = Color.black;
                            GameController.Coler_Manager[i] = 2;
                            break;
                        case 1:
                            GameController.component_cash[i].color = Color.white;
                            GameController.Coler_Manager[i] = 1;
                            break;
                    }
                }

            }
        }
        else
        {

            ClickPurchaseObject();
        }

    }

    void Selected()
    {

        piano_automove(min_onmouse, max_onmouse);

        FuncKeyClicked(true);

        if (MatchmakingView.roomenterdflag)
        {
            photonView.RPC(nameof(all_clrst), RpcTarget.All, array_dammy);

        }
        else
        {
            all_clrst(array_dammy);

        }

        Get_ArrayScale1();
        Color_Blue_Yellow();
        array_scale1[0] = index_onmouse;

        if (MatchmakingView.roomenterdflag)
        {
            if (BoolControl)
            {

                play(1);
            }
            else
            {
                play(0);    //オンラインモードで生徒発声

            }
        }

        else
        {

            if (BoolControl)
            {
                play(3);

            }
            else
            {
                play(2);    //オンラインモードで生徒発声

            }
        }
    }

    void AutoscaleShiftArray()
    {

        if (AutoScaleShift != 0 && LastScale == true)
        {
            array_scale1[0] = array_scale1[0] + AutoScaleShift;
        }
        else
        {
            array_scale1[0] = array_scale1[0] + 2 * AutoScaleShift;

        }
    }



}

