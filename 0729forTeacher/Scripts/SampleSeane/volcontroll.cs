using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class volcontroll : MonoBehaviour
{
    public static float newSliderValue = 1;
    float SliderValue;
    [SerializeField] private Slider volumeSlider = null;

    // Start is called before the first frame update
    void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1);
        newSliderValue = volumeSlider.value;
    }
    public void SoundSliderOnValueChange(float SliderValue)
    {    // 音楽の音量をスライドバーの値に変更
        newSliderValue = SliderValue;
        PlayerPrefs.SetFloat("Volume", newSliderValue);
        PlayerPrefs.Save();
    }
}
