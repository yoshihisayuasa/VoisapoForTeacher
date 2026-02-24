using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class volcontroll : MonoBehaviour
{
    public static float newSliderValue = 1;

    // Start is called before the first frame update

    public void SoundSliderOnValueChange(float SliderValue)
    {    // 音楽の音量をスライドバーの値に変更
        newSliderValue = SliderValue;
    }
}
