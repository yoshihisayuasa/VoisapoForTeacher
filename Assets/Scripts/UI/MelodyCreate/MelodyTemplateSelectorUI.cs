using Assets.Scripts.Domain.StaticValues;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.MelodyCreate
{
    public sealed class MelodyTemplateSelectorUI : MonoBehaviour
    {
        [SerializeField] private NoteSequencerUI _sequencer;
        [SerializeField] private TMP_Dropdown _dropdown;

        private void Start()
        {
            PopulateDropdown();
            _dropdown.onValueChanged.AddListener(OnValueChanged); // 初期値セット後に登録
        }

        private void PopulateDropdown()
        {
            var options = new List<TMP_Dropdown.OptionData> { new("") };
            foreach (var template in MelodyTemplateLibrary.All)
            {
                options.Add(new(template.Name));
            }

            _dropdown.ClearOptions();
            _dropdown.AddOptions(options);
            _dropdown.value = 0;
            _dropdown.RefreshShownValue();
            _dropdown.captionText.text = "Template";
        }

        private void OnValueChanged(int index)
        {
            if (index == 0) return;
            _sequencer.LoadTemplate(MelodyTemplateLibrary.All[index - 1]);
        }

        public void ResetSelection()
        {
            _dropdown.value = 0; _dropdown.RefreshShownValue();
            _dropdown.captionText.text = "Template";
        }
    }
}
