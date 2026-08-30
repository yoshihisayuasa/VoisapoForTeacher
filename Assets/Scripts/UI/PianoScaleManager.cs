using Assets.Scripts.Domain.ValueObjects;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// ピアノ鍵盤の表示倍率の管理クラス（シングルトン）。
    /// 倍率の唯一の情報源であり、端末への保存もここが担う。
    /// </summary>
    public sealed class PianoScaleManager
    {
        private const string PrefsKey = "PianoScale";

        public static PianoScaleManager Instance { get; } = new PianoScaleManager();

        private PianoScale _scale;
        private readonly Subject<PianoScale> _scaleChanged = new();

        public PianoScale Scale => _scale;
        public Observable<PianoScale> ScaleChanged => _scaleChanged;

        private PianoScaleManager()
        {
            _scale = new PianoScale(PlayerPrefs.GetFloat(PrefsKey, PianoScale.Default));
        }

        public void SetScale(float value)
        {
            _scale = new PianoScale(value);
            PlayerPrefs.SetFloat(PrefsKey, _scale.Value);
            PlayerPrefs.Save();
            _scaleChanged.OnNext(_scale);
        }

        /// <summary>
        /// 現在の倍率から相対的にズームする（呼び出し側が現在値を読んで計算しないため）。
        /// </summary>
        public void Zoom(float delta)
        {
            SetScale(_scale.Zoom(delta).Value);
        }

        public void ZoomIn(float step)
        {
            SetScale(_scale.ZoomIn(step).Value);
        }

        public void ZoomOut(float step)
        {
            SetScale(_scale.ZoomOut(step).Value);
        }
    }
}
