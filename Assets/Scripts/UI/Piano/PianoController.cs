using R3;
using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.Infrastructure;
using Assets.Scripts.UI;
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

        private PianoKeyHighlighter _highlighter;
        private PianoKeyPlayback _playback;
        private Dictionary<PianoNoteEnum, PianoKeyUI> _keyDict;

        // キーボード操作・受信など、ポインタ以外からの押下／離鍵を注入するストリーム。
        private Subject<PianoNote> _keyClicks;
        private Subject<PianoNote> _keyUps;

        // 根音(SelectedKey)として確定した押下を外部へ通知するストリーム。
        private Subject<PianoNote> _rootKeyPressed;

        // 選択鍵盤（根音）の変更を外部へ通知するストリーム。発火点は SelectKey のみ。
        private Subject<PianoNote> _selectionChanged;

        // 外部公開する鍵盤イベント（押下確定・離鍵・選択変更）。Awake で組み立てる。
        public Observable<PianoNote> OnRootKeyPressedAsObservable { get; private set; }
        public Observable<PianoNote> OnAnyKeyUpAsObservable { get; private set; }
        public Observable<PianoNote> OnSelectionChangedAsObservable { get; private set; }

        [SerializeField]
        [Tooltip("ピアノ鍵盤を含む ScrollRect (水平スクロール)")] private ScrollRect _scrollRect;

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
            _highlighter = new PianoKeyHighlighter(_keyDict);
            _playback = new PianoKeyPlayback(_keyDict);

            _keyClicks        = new Subject<PianoNote>();
            _keyUps           = new Subject<PianoNote>();
            _rootKeyPressed   = new Subject<PianoNote>();
            _selectionChanged = new Subject<PianoNote>();

            // 鍵盤のポインタ入力検知（PianoKeyInputUI）は先生・生徒どちらのビルドにも存在する。
            var keyInputs = _pianoKeys
                .Select(k => k.GetComponent<PianoKeyInputUI>())
                .Where(k => k != null)
                .ToList();

            Observable<PianoNote> pointerDowns  = keyInputs.Select(k => k.OnClickKeyAsObservable).Merge();
            Observable<PianoNote> pointerUps    = keyInputs.Select(k => k.OnPointerUpAsObservable).Merge();
            Observable<PianoNote> pointerEnters = keyInputs.Select(k => k.OnPointerEnterAsObservable).Merge();

            // 再生は「根音(SelectedKey)を確定してから」通知する。生の押下入力を一旦受けて
            // SelectKey 後に _rootKeyPressed へ流し直すことで、購読者の登録順に依存せず
            // すべての購読者が確定後の SelectedKey を読めるようにする。
            // 先生はポインタ押下も起点になるが、生徒のポインタ押下はメロディ・選択・ハイライトへは流さず、
            // 下の単音ローカル再生だけに使う（生徒がこのパイプラインへ流すのはネットワーク受信＝_keyClicksのみ）。
            Observable<PianoNote> rootKeyPresses = AppMode.IsTeacher? pointerDowns.Merge(_keyClicks) : _keyClicks;
            rootKeyPresses.Subscribe(note =>
            {
                SelectKey(note);
                _rootKeyPressed.OnNext(note);
            }).AddTo(this);

            OnRootKeyPressedAsObservable   = _rootKeyPressed;
            OnAnyKeyUpAsObservable         = AppMode.IsTeacher ? pointerUps.Merge(_keyUps) : _keyUps;
            OnSelectionChangedAsObservable = _selectionChanged;

            // 先生はマウスホバーで根音を移動できる（キーボード操作＝MoveSelectionと対称）。
            // 選択への追従（ハイライト更新等）は OnSelectionChanged の購読側が行う。
            // 生徒はホバーで選択を動かさない。
            if (AppMode.IsTeacher)
            {
                pointerEnters.Subscribe(SelectKey).AddTo(this);
            }

            // 生徒は鍵盤を押している間だけ、その鍵盤の音をローカルで鳴らす（ハイライト無し）。
            if (!AppMode.IsTeacher)
            {
                pointerDowns.Subscribe(PlayKeySound).AddTo(this);
                pointerUps.Subscribe(StopKeySound).AddTo(this);
            }

            // ネットワーク送信はインフラ層（PianoNetworkGateway）が公開ストリームを購読して行う。
            // このクラスはネットワークの存在を知らない。
        }

        private void Start()
        {
            SelectKey(PianoNote.DefaultRoot);
        }

        /// <summary>
        /// シングルトン重複検知で Awake を早期 return した場合、_highlighter が未初期化のため nullチェックが必要
        /// </summary>
        private void OnDisable()
        {
            _highlighter?.Deselect();
        }

        /// <summary>
        /// // シングルトン重複検知で Awake を早期 return した場合、Subject が未初期化のため nullチェックが必要
        /// </summary>
        private void OnDestroy()
        {
            _keyClicks?.Dispose();
            _keyUps?.Dispose();
            _rootKeyPressed?.Dispose();
            _selectionChanged?.Dispose();
        }

        /// <summary>
        /// 選択中の鍵盤を delta だけ移動する。
        /// キーボード操作（先生専用の PianoKeyboardInput）から呼ぶ。
        /// </summary>
        public void MoveSelection(int delta)
        {
            Debug.Assert(SelectedKey != null, "SelectedKey is null");
            SelectKey(SelectedKey.MovedBy(delta, KeyCount));
        }

        public PianoNote SelectedKey => _highlighter.SelectedKey;

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
        public void Play(PianoNote pressedKey, bool isPlaySound)
        {
            _playback.Play(pressedKey, isPlaySound);
        }

        /// <summary>
        /// 指定鍵盤の音だけを鳴らす（視覚変化なし）。生徒が鍵盤を押している間の単音演奏用。
        /// </summary>
        public void PlayKeySound(PianoNote note)
        {
            _playback.PlayKeySound(note);
        }

        /// <summary>
        /// 指定鍵盤の音だけをフェードアウト停止する（視覚変化なし）。生徒が鍵盤を離したとき用。
        /// </summary>
        public void StopKeySound(PianoNote note)
        {
            _playback.StopKeySound(note);
        }

        /// <summary>
        /// 鍵盤を停止し、視覚をデフォルト色に戻す。
        /// </summary>
        public void Stop(PianoNote key)
        {
            _playback.Stop(key);
        }

        /// <summary>
        /// 鍵盤を停止し、「演奏済み」色を付けて残す。
        /// </summary>
        public void StopAndMarkPlayed(PianoNote key)
        {
            _playback.StopAndMarkPlayed(key);
        }

        public void StopAllKeys(bool setKeyVisual)
        {
            _playback.StopAllKeys(setKeyVisual);
        }

        /// <summary>
        /// 指定したキー範囲が viewport に収まるよう ScrollRect をスクロールする。
        /// </summary>
        public void EnsureRangeVisible(PianoKeyRange range)
        {
            if (!range.IsWithinKeyboard(KeyCount))
            {
                return;
            }

            var minRT = GetKeyUI(range.Min).transform as RectTransform;
            var maxRT = GetKeyUI(range.Max).transform as RectTransform;
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

        private PianoKeyUI GetKeyUI(PianoNote pressedKey)
        {
            return _keyDict[pressedKey.Note];
        }

        /// <summary>
        /// 選択鍵盤（根音）を変更する。あらゆる経路（ポインタ・キーボード・再生系・受信）の
        /// 選択変更はここを通り、OnSelectionChanged で購読側へ通知される。
        /// </summary>
        public void SelectKey(PianoNote key)
        {
            _highlighter.SelectKey(key);
            _selectionChanged.OnNext(key);
        }

        public void Deselect()
        {
            _highlighter.Deselect();
        }

        public void SetHighlight(PianoKeyRange range)
        {
            _highlighter.SetHighlight(range);
        }

        /// <summary>
        /// 現在のハイライト範囲（Min/Max）を解除する。再生可能範囲外のキーを選択したときに呼ぶ。
        /// </summary>
        public void ClearHighlight()
        {
            _highlighter.ClearHighlight();
        }

    }

}
