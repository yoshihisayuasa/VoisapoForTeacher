using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public static class WaitForSecondsCache
{
    private static Dictionary<float, WaitForSeconds> _dic = new Dictionary<float, WaitForSeconds>();

    private static WaitForSeconds Get(float seconds)
    {
        if (!_dic.ContainsKey(seconds))
        {
            _dic.Add(seconds, new WaitForSeconds(seconds));
        }

        return _dic[seconds];
    }

    public static IEnumerator Wait(float seconds)
    {
        var wait = Get(seconds);
        yield return wait;
    }
}