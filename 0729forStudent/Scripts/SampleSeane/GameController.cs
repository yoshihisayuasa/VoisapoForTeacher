
//次やること：Up もしくはダウンしたらキーの色を変更
//クリック時もキーの色を変更
//キーに色を付与
//https://qiita.com/yoship1639/items/7339a6201b44a24fbdfe#pixel-light-count-%E3%82%92%E5%B0%8F%E3%81%95%E3%81%8F%E3%81%99%E3%82%8B%E3%82%82%E3%81%97%E3%81%8F%E3%81%AF0%E3%81%AB%E3%81%99%E3%82%8B
//ビルド設定

using System.Collections;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

//登録するスケールの音数は30を限度と設定

public class GameController : MonoBehaviourPunCallbacks
{
    AudioSource sounds;

    [SerializeField] GameObject PopUp;
    //10*10のint型２次元配列を定義

    [SerializeField] GameObject piano;

    private Camera camera_object;

    string[] array_keyname = new string[88];
    [SerializeField] Text trainerside;
    [SerializeField] Text studentside;
    //GameObject piano;
    GameObject[] object2 = new GameObject[88];
    //public static SpriteRenderer[] component_cash = new SpriteRenderer[88];
    public static Image[] component_cash = new Image[88];

    public static int[] array_scale1 = new int[110];
    bool BoolTrainerDemo;
    public bool KeyClicked = false;

    public static int[] Coler_Manager_init = new int[88] { 1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,
                                            1,2,1,2,1,1,2,1,2,1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,1,2,1,2,1,1,2,1,2,1,2,1,1};



    public static int[] Coler_Manager = new int[88]; //white=1 black=2 red=3 pink=4 darkpink=5
    // public static string[] Coler_Manager_BlueYellow = new string[88];

    Transform piano_transform;
    Transform[] object2_transform = new Transform[88];
    AudioClip[] clip = new AudioClip[88];
    AudioClip metronome;


    Color darkpink = new Color(0.675f, 0.463f, 0.451f, 1.0f);
    Color pink = new Color(1, 0.5f, 0.6f, 1);
    Color darkbkack = new Color(0.313f, 0.313f, 0.313f, 1);


    public static int AutoScaleShift = 0; //-1 Dwn  0 Off 1 Up
    bool InCord = false;

    int Playside = 0;//1が生徒。2がトレーナー
    bool LastScale = false;

    bool ChangeScaleDirection = false;
    bool AfterTrainerSide = false;
    int time = 0;


    float camewrld_max = 0;
    float camewrld_min = 0;
    Vector3 max_keypos;
    Vector3 min_keypos;
    int scale_max = 0;
    int scale_min = 0;
    Vector3 V3;
    float middle = 0;



    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;//startから移動。ロックしてしまうデバイスがある不具合対策。
        Application.targetFrameRate = 60; //60FPSに設定
    }
    void Start()
    {

        PopUp.SetActive(true);

        camera_object = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        sounds = GetComponent<AudioSource>();
        piano_transform = piano.transform;

        //コンポーネントの一括取得
        for (int i = 0; i < 88; i++)
        {
            object2[i] = GameObject.Find(i.ToString());
            object2_transform[i] = object2[i].transform;
            component_cash[i] = object2[i].GetComponent<Image>();
            array_keyname[i] = object2[i].name;
            Coler_Manager[i] = Coler_Manager_init[i];
            clip[i] = Resources.Load("ogg/" + i, typeof(AudioClip)) as AudioClip;
        }
        metronome = Resources.Load("metronome/metronome", typeof(AudioClip)) as AudioClip;

        V3 = new Vector3(piano_transform.position.x, piano_transform.position.y, piano_transform.position.z);


    }



    void Update()
    {


        if (((AutoScaleShift != 0 && LastScale == true) || ChangeScaleDirection && InCord) && KeyClicked)
        {
            if (!BoolTrainerDemo)
            {
                //トレーナーのデモが終わったら中に入る。生徒のAutoに入っていたらfalseにかわる。入っていなければ先生側のDelayが働く
                //懸念しているのはAfterTrainerSideがTrueになった状態で、先生がクリックしてDelayにならないか。
                //先生側が再生しているときはAfterTreinerSideはtrueにならない。
                //先生側で再生される前にtrueになることはあるのか？
                //ない。先生側で再生される前は生徒側で再生されるので。

                photonView.RPC(nameof(all_clrst), RpcTarget.All, array_scale1);

                if (Playside == 1)
                {

                    AutoscaleShiftArray();
                }
                else
                {

                    AfterTrainerSide = true;

                }

                play();

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

            else
            {
                photonView.RPC(nameof(RPCPushControlRecive), RpcTarget.Others, true);

            }



            if (ChangeScaleDirection == true)
            {
                ChangeScaleDirection = false;

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
        trainerside.color = darkbkack;
        studentside.color = darkbkack;
        if (sounds != null)
        {
            sounds.Stop();
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

        photonView.RPC(nameof(RPCLasetScale), RpcTarget.All, false);


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


        InCord = true;

        if (AfterTrainerSide)
        {
            yield return WaitForSecondsCache.Wait(0.15f);
            AfterTrainerSide = false;

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


        photonView.RPC(nameof(RPCLasetScale), RpcTarget.All, true);


    END:;

    }

    private IEnumerator IEPLAYSIDE_Trainer(int[] array_scale1)
    {
        time = array_scale1[60]; //和音の拍数は60番目に入っている


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
        trainerside.color = darkbkack;

    END:;

    }

    private IEnumerator IEPLAYSIDE_Student(int[] array_scale1)
    {
        time = array_scale1[60]; //和音の拍数は60番目に入っている


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
        studentside.color = darkbkack;
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

        if (AfterTrainerSide)
        {
            yield return WaitForSecondsCache.Wait(0.15f);

        }

        piano_automove(array_scale1);



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



        //色を戻す

        for (int i = 0; i < 3; i++)
        {
            if (array_scale1[60 + i] == 0)
            {
                break;
            }

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




    void piano_automove(int[] array_scale1)
    {



        scale_max = array_scale1[1] + array_scale1[0];
        scale_min = array_scale1[2] + array_scale1[0];

        if (scale_max < 88 && scale_min > -1 && scale_min < 88 && scale_max > -1)
        {

            max_keypos = object2_transform[scale_max].position;
            min_keypos = object2_transform[scale_min].position;

            camewrld_max = camera_object.WorldToViewportPoint(max_keypos).x;
            camewrld_min = camera_object.WorldToViewportPoint(min_keypos).x;

            if (camewrld_max > 1 || (camewrld_min < 0))
            {
                middle = (max_keypos.x + min_keypos.x) / 2;
                V3.x = piano_transform.position.x - middle;

                piano_transform.position = V3;
            }
        }
    }



    void swich_color(int number)
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


    public void PoPUPbtn()
    {
        PopUp.SetActive(false);

    }


    public void VolPlayTest()
    {
        RPCSTOPALL();
        StartCoroutine("IE_VolPlayTest");
    }


    public IEnumerator IE_VolPlayTest()
    {

        int[,] arr_fvtn = new int[,] { { 0, 3 }, { 4, 3 }, { 7, 3 }, { 0, 1 }, { 2, 1 }, { 4, 1 }, { 5, 1 }, { 7, 1 }, { 5, 1 }, { 4, 1 }, { 2, 1 }, { 0, 2 } };//初めの3つは和音

        sounds.Stop();


        for (int i = 0; i < 3; i++)  //和音を再生
        {
            if (arr_fvtn[0, i] == 0)
            {
                break;
            }

            sounds.PlayOneShot(clip[36 + arr_fvtn[i, 1]], volcontroll.newSliderValue);

        }

        for (int i = 0; i < arr_fvtn[0, 1]; i++)  //メトロノーム再生
        {
            sounds.PlayOneShot(metronome, volcontroll.newSliderValue);
            yield return WaitForSecondsCache.Wait(BPM.bpm_time());

        }

        sounds.Stop();


        for (int i = 3; i < 12; i++)
        {
            if (arr_fvtn[i, 1] == 0)
            {
                break;
            }
            sounds.PlayOneShot(clip[36 + arr_fvtn[i, 0]], volcontroll.newSliderValue);
            yield return WaitForSecondsCache.Wait(BPM.bpm_time() * arr_fvtn[i, 1]);

            sounds.Stop();
        }

    }


    private void OnApplicationQuit()
    {
        // 自動スリープをシステム設定に戻す
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    [PunRPC]
    void RPCLasetScale(bool LastScale1)
    {
        LastScale = LastScale1;
    }
    [PunRPC]
    void RPCInCord(bool InCordBool)
    {
        InCord = InCordBool;

    }

    private void play()
    {

        photonView.RPC(nameof(RPCSTOPALL), RpcTarget.All);
        photonView.RPC(nameof(RPCCOLER), RpcTarget.All, array_scale1);
        photonView.RPC(nameof(RPC_IEPLAYSIDE_Studenet), RpcTarget.All, array_scale1);
        //生徒側で再生の表示

        if (Playside != 1)
        {
            FuncPlayside(1);
        }

        RPCPLAY(array_scale1);

        if (EarPhoneMode.EarPhoneModeBool)
        {
            photonView.RPC(nameof(RPCIEPLAY_Waon), RpcTarget.Others, array_scale1);
        }


    }





    [PunRPC]
    public void RPCBtnChangeScaleDirection(int NewAutoScaleShift, bool NewChangeScaleDirection)
    {


        AutoScaleShift = NewAutoScaleShift;
        ChangeScaleDirection = NewChangeScaleDirection;

    }

    [PunRPC]
    public void RPCPushControlRecive(bool TrueFalse)
    {
        BoolTrainerDemo = TrueFalse;
    }

    [PunRPC]
    public void RPCPushControl(bool TrueFalse)
    {
        BoolTrainerDemo = TrueFalse;
    }


    private void FuncPlayside(int PlaysideNumber)
    {
        photonView.RPC(nameof(RPCPlayside1), RpcTarget.All, PlaysideNumber);


    }


    [PunRPC]
    public void RPCPlayside1(int PlaysideNumber)
    {
        Playside = PlaysideNumber;

    }



    [PunRPC]
    public void RPCKeyClicked(bool TrueFalse)
    {
        KeyClicked = TrueFalse;
    }




    [PunRPC]
    private void RPCIEPLAY_Waon(int[] array_scale2)
    {

    }


}
