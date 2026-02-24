using Scripts.UI.Melody;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DomainMelody = Scripts.Domain.Melody;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 起動時に MelodyManager のリストからメロディーボタンを自動生成
    /// </summary>
    public sealed class MelodyListBuilder : MonoBehaviour
    {
        [SerializeField] private Transform _content;    // ScrollView/Viewport/Content
        [SerializeField] private Button _buttonPrefab;  // メロディー選択用ボタンのプレハブ

        private void Start()
        {
            // MelodyManager.Start() のJSON読み込み完了を待ってから生成
            StartCoroutine(BuildWhenReady());
        }

        private System.Collections.IEnumerator BuildWhenReady()
        {
            // インスタンス生成待ち
            while (MelodyManager.Instance == null)
            {
                yield return null;
            }

            // Startが走るまで1フレーム待機（JSONロード完了待ち）
            yield return null;

            var manager = MelodyManager.Instance;
            var melodies = manager.GetAllMelodies();
            if (melodies == null || melodies.Count == 0)
            {
                Debug.LogWarning("MelodyListBuilder: メロディーが見つかりません。JSONを確認してください。");
                yield break;
            }

            Build();
        }

        public void Build()
        {
            var manager = MelodyManager.Instance;
            if (manager == null || _content == null || _buttonPrefab == null)
            {
                return;
            }

            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                Destroy(_content.GetChild(i).gameObject);
            }

            var melodies = manager.GetAllMelodies();
            for (int i = 0; i < melodies.Count; i++)
            {
                int idx = i;
                DomainMelody melody = melodies[idx];

                var btn = Instantiate(_buttonPrefab, _content);
                var text = btn.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = string.IsNullOrEmpty(melody.Name) ? $"Melody {idx + 1}" :melody.Name;
                }

                btn.onClick.AddListener(() =>
                {
                    manager.SetCurrentMelody(melody);
                });
            }
        }
    }
}