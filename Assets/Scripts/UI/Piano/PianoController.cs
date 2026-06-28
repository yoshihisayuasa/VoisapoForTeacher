using R3;
using AsseScripts.Domain;
using AsseScripts.Infrastructure;
using AsseScripts.UI;
using AsseScripts.UI.Piano;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// 役割：鍵盤の入力管理・イベント伝達
    /// </summary>
    public sealed class PianoController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("鍵盤一覧")] private List<PianoKeyUI>
            _pianoKeys;

        private (PianoNote Min, PianoNote Max)? _highlightedKeys;
        private PianoNote _selectedKey;
        private readonly HashSet<PianoNote> _playingKeys = new();
        private readonly HashSet<PianoNote> _coloredKeys = new();
        private Dictionary<PianoNoteEnum, PianoKeyUI> _keyDict;

        // キーボード操作・受信など、ポインタ以外からの押下／離鍵を注入するストリーム。
        private Subject<PianoNote> _keyClicks;
        private Subject<PianoNote> _keyUps;
        private Subject<PianoNote> _virtualKeyEnters;

        // 選択確定後にクリックを外部へ通知するストリーム。
        private Subject<PianoNote> _anyKeyClick;

        [SerializeField]
        [Tooltip("ピアノ鍵盤を含む ScrollRect (水平スクロール)")] private ScrollRect _scrollRect;

        [SerializeField]
        [Tooltip("鍵盤入力のPhoton通信ゲートウェイ")] private PianoNetworkGateway _network;
        public void ApplySoundSet(PianoSoundSet soundSet)
        {
            foreach (var keyUI in _pianoKeys)
            {
                keyUI.SwapAudioClip(soundSet.GetClip(keyUI.NoteEnum));
            }
        }

        public static PianoController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _keyDict = new Dictionary<PianoNoteEnum, PianoKeyUI>(_pianoKeys.Count);
            foreach (var key in _pianoKeys)
            {
                _keyDict[key.NoteEnum] = key;
            }

            _keyClicks        = new Subject<PianoNote>();
            _keyUps           = new Subject<PianoNote>();
            _virtualKeyEnters = new Subject<PianoNote>();
            _anyKeyClick      = new Subject<PianoNote>();

            // 鍵盤クリックの入力検知は先生ビルドにのみ存在する（PianoKeyInputUI参照）。
            var keyInputs = AppMode.IsTeacher
                ? _pianoKeys.Select(k => k.GetComponent<PianoKeyInputUI>())
                : Enumerable.Empty<PianoKeyInputUI>();

            var pointerUps = keyInputs.Select(k => k.OnPointerUpAsObservable).Merge();

            // 再生は「根音(SelectedKey)を確定してから」通知する。生のクリックを一旦受けて
            // SelectKey 後に _anyKeyClick へ流し直すことで、購読者の登録順に依存せず
            // すべての購読者が確定後の SelectedKey を読めるようにする。
            var rawClicks = keyInputs.Select(k => k.OnClickKeyAsObservable).Merge().Merge(_keyClicks);
            rawClicks.Subscribe(note =>
            {
                SelectKey(note);
                _anyKeyClick.OnNext(note);
            }).AddTo(this);

            OnAnyKeyClickAsObservable = _anyKeyClick;
            OnAnyKeyUpAsObservable    = pointerUps.Merge(_keyUps);
            OnAnyKeyEnterAsObservable = keyInputs.Select(k => k.OnPointerEnterAsObservable).Merge().Merge(_virtualKeyEnters);

            // 送信可否（先生のみ）はゲートウェイ側で判定。受信由来も同じ経路を通るが、
            // 生徒は送信が権限で弾かれるためループしない。
            // メロディ作成シーンなど Photon を持たないシーンでは _network が未設定のため購読しない。
            if (_network != null)
            {
                OnAnyKeyClickAsObservable.Subscribe(note => _network.SendKeyDown(note)).AddTo(this);
                OnAnyKeyUpAsObservable.Subscribe(note => _network.SendKeyUp(note)).AddTo(this);
            }
        }

        private void Start()
        {
            SelectKey(new PianoNote(PianoNoteEnum.C4));
        }

        private void OnDisable()
        {
            Deselect();
        }

        /// <summary>
        /// // シングルトン重複検知で Awake を早期 return した場合、Subject が未初期化のため nullチェックが必要
        /// </summary>
        private void OnDestroy()
        {
            _keyClicks?.Dispose();
            _keyUps?.Dispose();
            _virtualKeyEnters?.Dispose();
            _anyKeyClick?.Dispose();
        }

        /// <summary>
        /// 選択中の鍵盤を delta だけ移動し、移動先を仮想的な「ホバー進入」として通知する。
        /// キーボード操作（先生専用の PianoKeyboardInput）から呼ぶ。
        /// </summary>
        public void MoveSelection(int delta)
        {
            Debug.Assert(_selectedKey != null, "_selectedKey is null");
            int currentIndex = _selectedKey.Index;
            int nextIndex = Mathf.Clamp(currentIndex + delta, 0, KeyCount - 1);
            var nextKey = new PianoNote((PianoNoteEnum)nextIndex);

            SelectKey(nextKey);
            _virtualKeyEnters.OnNext(nextKey);
        }

        public Observable<PianoNote> OnAnyKeyUpAsObservable { get; private set; }
        public Observable<PianoNote> OnAnyKeyClickAsObservable { get; private set; }
        public Observable<PianoNote> OnAnyKeyEnterAsObservable { get; private set; }

        public PianoNote SelectedKey
        {
            get => _selectedKey;
            set => SelectKey(value);
        }

        /// <summary>
        /// ポインタクリック以外（ネットワーク受信・キーボード操作等）からの鍵盤押下を、
        /// 自分の鍵盤がクリックされたのと同等に扱う。
        /// </summary>
        public void PressKey(PianoNote note)
        {
            _keyClicks.OnNext(note);
        }

        /// <summary>
        /// ポインタクリック以外（ネットワーク受信・キーボード操作等）からの鍵盤リリースを、
        /// 自分の鍵盤を離したのと同等に扱う。
        /// </summary>
        public void ReleaseKey(PianoNote note)
        {
            _keyUps.OnNext(note);
        }

        public int KeyCount => _pianoKeys.Count;

        /// <summary>
        /// 視覚は常に更新し、音再生は isPlaySound で制御する。
        /// </summary>
        public void Play(PianoNote pressedKey, bool isPlaySound, float volume)
        {
            _playingKeys.Add(pressedKey);
            var key = GetKeyUI(pressedKey);
            key.SetPlayingVisual();
            if (isPlaySound)
            {
                key.PlaySound(volume * SoundSourceSwitcher.Instance.CurrentVolumeMultiplier);
            }
        }

        public void Stop(PianoNote key, bool setPlayedColor)
        {
            _playingKeys.Remove(key);
            if (setPlayedColor)
            {
                _coloredKeys.Add(key);
            }
            else
            {
                _coloredKeys.Remove(key);
            }
            var keyUI = GetKeyUI(key);
            keyUI.StopSound(BPMManager.Instance.SecondPerBeat * 0.6f);
            keyUI.SetKeyVisual(setPlayedColor);
        }

        public void StopMelody(bool setKeyVisual)
        {
            float fadeOut = BPMManager.Instance.SecondPerBeat * 0.6f;
            foreach (var key in _playingKeys)
            {
                var keyUI = GetKeyUI(key);
                keyUI.StopSound(fadeOut);
                if(setKeyVisual)
                {
                    keyUI.SetKeyVisual(false);
                }
                else
                {
                    _coloredKeys.Add(key);
                }
            }
            _playingKeys.Clear();

            if (setKeyVisual)
            {
                foreach (var key in _coloredKeys)
                {
                    var keyUI = GetKeyUI(key);
                    keyUI.SetKeyVisual(false);
                }
                _coloredKeys.Clear();
            }
        }

        /// <summary>
        /// 指定したキー範囲が viewport に収まるよう ScrollRect をスクロールする。
        /// </summary>
        public void EnsureRangeVisible(PianoNote minKey, PianoNote maxKey)
        {
            if (minKey.Index < 0 || maxKey.Index >= KeyCount)
            {
                return;
            }

            var minRT = GetKeyUI(minKey).transform as RectTransform;
            var maxRT = GetKeyUI(maxKey).transform as RectTransform;
            if (minRT == null || maxRT == null)
            {
                return;
            }

            _scrollRect.horizontalNormalizedPosition =
                ScrollRectRangeVisualizer.CalcNormalizedPosition(
                    _scrollRect.content,
                    _scrollRect.viewport,
                    minRT,
                    maxRT,
                    _scrollRect.horizontalNormalizedPosition);
        }
        /// <summary>
        /// インデックスが鍵盤配列の範囲内か判定します。
        /// </summary>

        public PianoKeyUI GetKeyUI(PianoNote pressedKey)
        {
            return _keyDict[pressedKey.Note];
        }

        public void SelectKey(PianoNote key)
        {
            if (_selectedKey != null)
            {
                GetKeyUI(_selectedKey).ResetAccentColor();
                RestoreHighlightIfNeeded(_selectedKey);
            }
            _selectedKey = key;
            GetKeyUI(_selectedKey).SetAccentColor();
        }

        public void Deselect()
        {
            if (_selectedKey == null) return;
            GetKeyUI(_selectedKey).ResetAccentColor();
            RestoreHighlightIfNeeded(_selectedKey);
            _selectedKey = null;
        }

        private void RestoreHighlightIfNeeded(PianoNote key)
        {
            if (!_highlightedKeys.HasValue) return;
            if (key == _highlightedKeys.Value.Min)
                GetKeyUI(key).SetMinHighlightColor();
            else if (key == _highlightedKeys.Value.Max)
                GetKeyUI(key).SetMaxHighlightColor();
        }

        public void SetHighlight(PianoNote key1, PianoNote key2)
        {
            var minKey = key1.Index <= key2.Index ? key1 : key2;
            var maxKey = key1.Index <= key2.Index ? key2 : key1;

            ClearHighlight();

            _highlightedKeys = (minKey, maxKey);

            if (minKey != _selectedKey) GetKeyUI(minKey).SetMinHighlightColor();
            if (maxKey != _selectedKey) GetKeyUI(maxKey).SetMaxHighlightColor();
        }

        /// <summary>
        /// 現在のハイライト範囲（Min/Max）を解除する。再生可能範囲外のキーを選択したときに呼ぶ。
        /// </summary>
        public void ClearHighlight()
        {
            if (!_highlightedKeys.HasValue) return;

            var oldMin = _highlightedKeys.Value.Min;
            var oldMax = _highlightedKeys.Value.Max;
            if (oldMin != _selectedKey) GetKeyUI(oldMin).ResetHighlightedColor();
            if (oldMax != _selectedKey) GetKeyUI(oldMax).ResetHighlightedColor();

            _highlightedKeys = null;
        }

    }

}
