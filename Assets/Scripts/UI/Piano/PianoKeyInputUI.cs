using AsseScripts.Domain;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// 鍵盤クリック・ホバーの入力検知（UI層）。先生・生徒どちらのビルドにも存在する。
    /// 検知結果の使われ方はビルドで異なる（PianoController参照）。
    /// </summary>
    public sealed class PianoKeyInputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
    {
        private readonly Subject<PianoNote> _onClickKeySubject = new();
        private readonly Subject<PianoNote> _onPointerUpSubject = new();
        private readonly Subject<PianoNote> _onPointerEnterSubject = new();

        private PianoKeyUI _keyUI;

        public Observable<PianoNote> OnClickKeyAsObservable => _onClickKeySubject;
        public Observable<PianoNote> OnPointerUpAsObservable => _onPointerUpSubject;
        public Observable<PianoNote> OnPointerEnterAsObservable => _onPointerEnterSubject;

        private void Awake()
        {
            _keyUI = GetComponent<PianoKeyUI>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _onClickKeySubject.OnNext(new PianoNote(_keyUI.NoteEnum));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _onPointerUpSubject.OnNext(new PianoNote(_keyUI.NoteEnum));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onPointerEnterSubject.OnNext(new PianoNote(_keyUI.NoteEnum));
        }

        private void OnDestroy()
        {
            _onClickKeySubject.Dispose();
            _onPointerUpSubject.Dispose();
            _onPointerEnterSubject.Dispose();
        }
    }
}
