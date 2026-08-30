using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.LessonRoom
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField, TextArea] private string _japanese;
        [SerializeField, TextArea] private string _english;

        private void Awake()
        {
            GetComponent<TMP_Text>().text = new LocalizedMessage(_japanese, _english).ForCurrentLanguage();
        }
    }
}
