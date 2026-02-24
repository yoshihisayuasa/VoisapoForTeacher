using UnityEngine;

public class iOSOnlyActive : MonoBehaviour
{
    void Start()
    {

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX //MacOSだったら処理

        //iOSの処理を書く
        this.gameObject.SetActive(true);
#else
        this.gameObject.SetActive(false);
#endif //iOSの処理範囲終わり
    }
}