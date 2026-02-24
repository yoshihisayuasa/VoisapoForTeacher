using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class EarPhoneMode : MonoBehaviourPunCallbacks
{

    public static bool EarPhoneModeBool = false;





    [PunRPC]
    public void EarPhoneModeMatch(bool EarPhoneModeBoolRecive) //マッチ時に同期させる処理
    {
        EarPhoneModeBool = EarPhoneModeBoolRecive;
    }


}
