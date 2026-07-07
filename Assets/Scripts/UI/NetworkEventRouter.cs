using Assets.Scripts.UI.MelodyUI;
using Assets.Scripts.UI.Piano;
using Assets.Scripts.Infrastructure;
using R3;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// ネットワークゲートウェイと各UIコンポーネントの橋渡し（采配役・UI層）。
    /// 受信イベントの各マネージャへの反映、UIイベントの送信、入室時の状態再送を担う。
    /// インフラ層（ゲートウェイ）はUIを知らないため、両者の対応表はこのクラスだけが持つ。
    /// PianoNetworkGateway と同じ GameObject に配置する。
    /// </summary>
    public sealed class NetworkEventRouter : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Photon通信ゲートウェイ")]
        private PianoNetworkGateway _gateway;

        private void Start()
        {
            SubscribeReceivedEvents();

            if (AppMode.IsTeacher)
            {
                SubscribeTeacherSends();
                SubscribeStudentPresence();
            }
        }

        /// <summary>
        /// 受信イベント → 各コンポーネントへの反映。
        /// 先生は RpcTarget.Others へ送るため、実際に受信するのは生徒側のみ。
        /// </summary>
        private void SubscribeReceivedEvents()
        {
            _gateway.KeyDownReceived
                .Subscribe(note => PianoController.Instance.PressKey(note))
                .AddTo(this);

            _gateway.KeyUpReceived
                .Subscribe(note => PianoController.Instance.ReleaseKey(note))
                .AddTo(this);

            _gateway.MelodyReceived
                .Subscribe(melody => MelodyManager.Instance.SetCurrentMelody(melody))
                .AddTo(this);

            _gateway.BpmReceived
                .Subscribe(bpm => BPMManager.Instance.SetValue(bpm))
                .AddTo(this);

            _gateway.StopReceived
                .Subscribe(_ => MelodyPlayer.Instance.FinishMelody())
                .AddTo(this);

            _gateway.SoundSetReceived
                .Subscribe(index =>
                {
                    if (SoundSourceSwitcher.Instance != null)
                    {
                        SoundSourceSwitcher.Instance.SwitchTo(index);
                    }
                })
                .AddTo(this);

            _gateway.SoundPlayStateReceived
                .Subscribe(state => SoundPlayManager.Instance.SetState(state))
                .AddTo(this);
        }

        /// <summary>
        /// 先生の操作・状態変化 → ネットワーク送信。
        /// PianoController.Instance は Awake で初期化済み（Start はすべての Awake の後に走る）。
        /// </summary>
        private void SubscribeTeacherSends()
        {
            var piano = PianoController.Instance;
            piano.OnRootKeyPressedAsObservable
                .Subscribe(_gateway.SendKeyDown)
                .AddTo(this);

            piano.OnAnyKeyUpAsObservable
                .Subscribe(_gateway.SendKeyUp)
                .AddTo(this);

            SoundPlayManager.Instance.OnStateChanged
                .Subscribe(_gateway.SendSoundPlayState)
                .AddTo(this);

            MelodyManager.Instance.MelodyChanged
                .Subscribe(_gateway.SendMelodySelection)
                .AddTo(this);

            BPMManager.Instance.BpmChanged
                .Subscribe(_gateway.SendBpm)
                .AddTo(this);

            if (SoundSourceSwitcher.Instance != null)
            {
                SoundSourceSwitcher.Instance.OnChanged
                    .Subscribe(_gateway.SendSoundSetSelection)
                    .AddTo(this);
            }
        }

        private void SubscribeStudentPresence()
        {
            // 起動時はイベント履歴に頼らず、ルームの現在の事実から在室状態を同期し直す。
            // 理由1: ゲートウェイはシーンローカルだが SoundPlayManager はアプリ寿命のため、
            //        メロディ作成シーン滞在中（ゲートウェイ不在）の入退室イベントを取りこぼしても
            //        古いフラグがシーン再生成後まで持ち越される。
            // 理由2: Photon では自分の入室時、既存メンバーについて OnPlayerEnteredRoom が発火しない
            //        （生徒が先に入室して待っているケースはイベントでは検知できない）。
            SoundPlayManager.Instance.SetStudentConnected(_gateway.HasConnectedStudent);

            // 生徒ありに切り替えると、実効状態が先生の意図（トグル）に戻る。
            _gateway.StudentPresenceChanged
                .Subscribe(connected => SoundPlayManager.Instance.SetStudentConnected(connected))
                .AddTo(this);

            _gateway.StudentJoined
                .Subscribe(_ => ResendCurrentState())
                .AddTo(this);
        }

        /// <summary>
        /// 後から入室した生徒に、先生の現在の状態（選択メロディ・音再生状態・BPM・音源）を再送する。
        /// 生徒は1人前提（ルームは MaxPlayers=2）のため、全員向け送信＝入室した生徒向けになる。
        /// 音再生状態は、新規起動の生徒なら既定値（鳴らさない）と一致するため冗長だが、
        /// 古い状態を保持したまま再入室した生徒（例：切断→再接続）を訂正できるのはこの再送だけ。
        /// 削除すると先生・生徒の両方で音が鳴る穴が開く。
        /// </summary>
        private void ResendCurrentState()
        {
            var melody = MelodyManager.Instance.CurrentMelody;
            if (melody != null)
            {
                _gateway.SendMelodySelection(melody);
            }

            _gateway.SendSoundPlayState(SoundPlayManager.Instance.IsSoundPlay);
            _gateway.SendBpm(BPMManager.Instance.Value);

            if (SoundSourceSwitcher.Instance != null)
            {
                _gateway.SendSoundSetSelection(SoundSourceSwitcher.Instance.CurrentIndex);
            }
        }
    }
}
