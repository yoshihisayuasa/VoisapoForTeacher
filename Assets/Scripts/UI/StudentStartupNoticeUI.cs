using System.Linq;
using Assets.Scripts.UI.Modal;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 生徒アプリの起動時に、音声トラブルを避けるための注意事項をポップアップで表示する。
    /// 消音モードの解除だけは見落とすと音がまったく鳴らないため、赤字で強調する。
    /// </summary>
    public sealed class StudentStartupNoticeUI : MonoBehaviour
    {
        private static readonly LocalizedMessage Notice = new(
            japanese: BulletList(
                Highlighted("iPhoneをご利用の場合は、消音モードをオフにしてください。"),
                "通話アプリのノイズキャンセリングをオフにしてください。設定方法は[Help]をご確認ください。",
                "1つのデバイスで通話アプリとVoisapoを併用する場合は、イヤホンを着用してください。"),
            english: BulletList(
                Highlighted("If you are using an iPhone, please turn off Silent Mode."),
                "Please turn off noise cancellation in your call app. See [Help] for how to do this.",
                "If you use a call app and Voisapo on the same device, please wear earphones."));

        /// <summary>各項目に中黒を付け、1行空けて並べた箇条書きにする。</summary>
        private static string BulletList(params string[] items)
        {
            return string.Join("\n\n", items.Select(item => $"・{item}"));
        }

        /// <summary>注意喚起の色を付けたリッチテキストにする。</summary>
        private static string Highlighted(string text)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(AppColors.Warning)}>{text}</color>";
        }

        /// <summary>
        /// ModalContainer は Awake で自身を登録するため、表示は Start で行う。
        /// </summary>
        private void Start()
        {
            ConfirmModalUI.Show(Notice.ForCurrentLanguage());
        }
    }
}
