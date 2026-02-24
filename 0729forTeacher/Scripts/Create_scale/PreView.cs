using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PreView : MonoBehaviour
{
    AudioSource sounds;
    // private string[] pianokey88 = GameController.pianokey88;
    AudioClip metronome;
    // Start is called before the first frame update
    void Start()
    {
        sounds = GetComponent<AudioSource>();
    }

    // Update is called once per frame

    public void PrePlay()
    {
        PCSTOP();
        StartCoroutine("IEPLAY");
    }

    public void PCSTOP()
    {
        StopCoroutine("IEPLAY");
    }

    public IEnumerator IEPLAY() //yieldreturnはupdateに呼び出される際には使える。関数として呼び出されたものにwaitがあってもupdateではwaitされない
    {
        int k = 0;

        //https://qiita.com/toRisouP/items/e6d4f114d434ee588044#%E5%87%A6%E7%90%86%E3%82%92n%E7%A7%92%E5%BE%8C%E3%81%AB%E5%AE%9F%E8%A1%8C%E3%81%97%E3%81%9F%E3%81%84
        //https://futabazemi.net/notes/script_function/
        string[] oggfilename;

        for (k = 0; k < 27; k++)
        {
            if (CreateScaleControll.int_clkdky[k, 1] == 0)
            {
                break;
            }

        }



        oggfilename = get_oggfilename();
        int oggfile_length = oggfilename.Length;
        AudioClip[] clip = new AudioClip[oggfile_length];

        sounds.Stop();


        for (int i = 0; i < k; i++)  //音をロード
        {
            clip[i] = Resources.Load("ogg/" + oggfilename[i], typeof(AudioClip)) as AudioClip;
        }

        metronome = Resources.Load("metronome/metronome", typeof(AudioClip)) as AudioClip;




        for (int i = 0; i < 3; i++)  //和音を再生
        {
            //和音を3つ以上入力していないとそもそもclipが存在しないから配列にアクセスできない
            if (clip[i] != null)
            {
                sounds.PlayOneShot(clip[i], volcontroll.newSliderValue);

            }

        }

        for (int i = 0; i < 3; i++)  //メトロノーム再生
        {
            sounds.PlayOneShot(metronome, volcontroll.newSliderValue);
            yield return WaitForSecondsCache.Wait(BPM.bpm_time());

        }

        sounds.Stop();

        for (int i = 3; i < k; i++)
        {


            sounds.PlayOneShot(clip[i], volcontroll.newSliderValue);
            yield return new WaitForSeconds(BPM.bpm_time() * CreateScaleControll.int_clkdky[i, 1]);
            sounds.Stop();

        }

    }


    private string[] get_oggfilename()
    {
        int k = 0;
        int newindex;
        string[] oggfilename = new string[27];

        for (k = 0; k < 27; k++)
        {
            if (CreateScaleControll.int_clkdky[k, 1] == 0)
            {
                break;
            }

        }
        for (int i = 0; i < k; i++)
        {
            newindex = CreateScaleControll.int_clkdky[i, 0];
            if (newindex < 88 && newindex >= 0)
            {
                // oggfilename[i] = pianokey88[newindex];
                oggfilename[i] = newindex.ToString();
            }
        }

        return oggfilename;

    }
}
