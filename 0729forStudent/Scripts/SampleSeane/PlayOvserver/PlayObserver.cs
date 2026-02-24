using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

//登録するスケールの音数は30を限度と設定

public class PlayObserver : MonoBehaviourPunCallbacks
{
    private bool _isStartPlaying=false;
    private bool _isEndPlaying = false;
    private bool _isEndCord = false;

    public bool IsStartPlaying
    {
        set
        {
            if (IsStartPlaying==false)
            {
                this._isStartPlaying = true;
            }
        }

        get { return this._isStartPlaying; }
    }

    class PropAccess
    {
        static void  Main(string[] args)
        {
            var t = new PlayObserver();
           

        }
        
    }
}
