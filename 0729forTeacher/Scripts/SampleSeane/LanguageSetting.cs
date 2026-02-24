using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;  //Localeを使うために追加
using UnityEngine.Localization.Settings;  //Localization.Settingsを使うために追加
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LanguageSetting : MonoBehaviour
{
    [SerializeField] Text Localizetext;
    [SerializeField] private Dropdown dropdown;


    string CurrentLocale;
    void Start()
    {

        //        CurrentLocale = PlayerPrefs.GetString("selected-locale", "initial");
        CurrentLocale = PlayerPrefs.GetString("selected-locale", "ja");
        // LocalizationSettings.SelectedLocale = Locale.CreateLocale(CurrentLocale);

        if (CurrentLocale == "en")
        {
            Localizetext.text = "English";
        }
        else
        {
            Localizetext.text = "日本語";

        }
        /*
        else (CurrentLocale == "ja")
        {
            Localizetext.text = "日本語";
        }
*/

    }
    // オプションが変更されたときに実行するメソッド
    public void Language()
    {
        Localizetext.text = "";
        //DropdownのValueが0のとき（赤が選択されているとき）
        if (dropdown.value == 1)
        {
            LocalizationSettings.SelectedLocale = Locale.CreateLocale("ja");
            PlayerPrefs.SetString("selected-locale", "ja");
            PlayerPrefs.Save();

        }
        else if (dropdown.value == 2)
        {
            LocalizationSettings.SelectedLocale = Locale.CreateLocale("en");
            PlayerPrefs.SetString("selected-locale", "en");
            PlayerPrefs.Save();

        }
        SceneManager.LoadScene("RoomID");


    }
}