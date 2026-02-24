using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;  //Localeを使うために追加
using UnityEngine.Localization.Settings;  //Localization.Settingsを使うために追加
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LocalizationScript : MonoBehaviour
{
    [SerializeField] Text Localizetext;

    string CurrentLocale;
    void Start()
    {

        // PlayerPrefs.SetString("selected-locale", "initial");
        CurrentLocale = PlayerPrefs.GetString("selected-locale", "initial");


        if (CurrentLocale != "initial")
        {
            LocalizationSettings.SelectedLocale = Locale.CreateLocale(CurrentLocale);

            SceneManager.LoadScene("RoomID");

        }

    }
    // オプションが変更されたときに実行するメソッド
    public void Japanese()
    {
        //DropdownのValueが0のとき（赤が選択されているとき）

        LocalizationSettings.SelectedLocale = Locale.CreateLocale("ja");
        PlayerPrefs.SetString("selected-locale", "ja");
        PlayerPrefs.Save();
        SceneManager.LoadScene("RoomID");


    }
    public void English()
    {
        LocalizationSettings.SelectedLocale = Locale.CreateLocale("en");
        PlayerPrefs.SetString("selected-locale", "en");
        PlayerPrefs.Save();
        SceneManager.LoadScene("RoomID");

    }
}