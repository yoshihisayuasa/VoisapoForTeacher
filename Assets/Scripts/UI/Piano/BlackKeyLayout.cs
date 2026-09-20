using Assets.Scripts.Domain.ValueObjects;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// 黒鍵を、隣り合う白鍵の境目へ配置する。
    ///
    /// 白鍵は HorizontalLayoutGroup が並べるが、黒鍵は IgnoreLayout で絶対座標に置くため、
    /// Spacing や鍵盤幅を変えると黒鍵だけが取り残される。
    /// ここで白鍵の実寸から毎回計算し直すことで、黒鍵の座標をシーンへ手で書き込まずに済ませる。
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class BlackKeyLayout : MonoBehaviour
    {
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(RepositionAfterLayout());
                return;
            }

            Reposition();
        }

        /// <summary>
        /// 起動直後は ContentSizeFitter が鍵盤全体の幅をまだ決めておらず、白鍵は中央揃えのため
        /// 本来と違う位置にいる。そこで読むと黒鍵だけ誤った座標に取り残されるので、
        /// レイアウトが確定する1フレーム後まで待つ。
        /// </summary>
        private IEnumerator RepositionAfterLayout()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            Reposition();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor で Spacing や鍵盤幅を編集した結果を即座に見せる。再生中は白鍵の並びが変わらないので不要。
        /// </summary>
        private void Update()
        {
            if (!Application.isPlaying)
            {
                Reposition();
            }
        }
#endif

        /// <summary>
        /// 白鍵の並びが確定してから黒鍵を置く。黒鍵の左右の白鍵は音の並び順で必ず隣接するため、
        /// 音名順に並べた鍵盤の前後を見れば境目が求まる（シーン上の子の順序には依存しない）。
        /// </summary>
        private void Reposition()
        {
            var keyboard = (RectTransform)transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(keyboard);

            var keys = GetComponentsInChildren<PianoKeyUI>(true)
                .OrderBy(key => (int)key.NoteEnum)
                .ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                if (!new PianoNote(keys[i].NoteEnum).IsSharp)
                {
                    continue;
                }

                PlaceOnBoundary(
                    (RectTransform)keys[i].transform,
                    (RectTransform)keys[i - 1].transform,
                    (RectTransform)keys[i + 1].transform);
            }
        }

        /// <summary>
        /// 黒鍵の中心を、左右の白鍵の隙間の中央へ合わせる（縦位置と大きさはシーンの設定のまま）。
        /// RectTransform.rect はピボット基準のため、localPosition に足すと親から見た端の座標になる。
        /// </summary>
        private void PlaceOnBoundary(RectTransform blackKey, RectTransform leftWhiteKey, RectTransform rightWhiteKey)
        {
            float gapCenterX =
                (leftWhiteKey.localPosition.x + leftWhiteKey.rect.xMax
                 + rightWhiteKey.localPosition.x + rightWhiteKey.rect.xMin) / 2f;

            var position = blackKey.localPosition;
            position.x = gapCenterX - blackKey.rect.center.x;
            blackKey.localPosition = position;
        }
    }
}
