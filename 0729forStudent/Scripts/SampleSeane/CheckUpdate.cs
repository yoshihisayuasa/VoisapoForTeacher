using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class CheckUpdate : MonoBehaviour
{

    // Use this for initialization

    [SerializeField]
    int ThisVersion;

    int LastVersion;

    [SerializeField]
    GameObject PopUp;

    void Start()
    {
        StartCoroutine(GetText());
    }



    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://voisapo.hatenablog.com/entry/2023/04/10/184740");
        yield return www.SendWebRequest();

        //        if (www.isNetworkError || www.isHttpError)
        //      {
        //        Debug.Log(www.error);
        //  }

        if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
        {
            Debug.Log(www.error);
        }

        else
        {
            string pattern = @"<p>(ver.*?)</p>";
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            System.Text.RegularExpressions.MatchCollection matches = r.Matches(www.downloadHandler.text);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string Version = match.Groups[1].Value.Replace("ver.", ""); // "ver"を除去して数字だけを取得


                LastVersion = int.Parse(Version); // 数値型に変換
            }

            CheckVersion();
        }
    }



    public void CheckVersion()
    {

        if (!ThisVersion.Equals(LastVersion))
        {//現在インストールされているアプリのバージョンは最新バージョンではない
            PopUp.SetActive(true);
            //「現在インストールされているアプリは最新じゃないのでアプデしてください」とか
            // ダイアログを表示したりしてね
        }
        else
        {
            PopUp.SetActive(false);

        }

    }

    public void UpdateButton()
    {
        string url = "";

#if UNITY_ANDROID || UNITY_EDITOR
        url = "https://play.google.com/store/apps/details?id=com.Voisapo.Voisapoforstudent";
#endif

#if UNITY_IOS||UNITY_EDITOR_OSX
            url = "https://apps.apple.com/app/apple-store/id6443756207";
           
#endif

        Application.OpenURL(url);//""の中には開きたいWebページのURLを入力します

    }

}
