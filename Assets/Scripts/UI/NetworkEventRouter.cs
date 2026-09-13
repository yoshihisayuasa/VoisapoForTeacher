using Assets.Scripts.Domain.ValueObjects;
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
            SubscribePerformanceSends();

            if (AppMode.IsTeacher)
            {
                SubscribeTeacherSends();
                SubscribeStudentPresence();
            }
            else
            {
                SubscribeRoomStateReset();
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

            _gateway.AutoKeyChangeReceived
                .Subscribe(state => AutoKeyChangeManager.Instance.ApplyRemote(state))
                .AddTo(this);

            _gateway.LoopKeyReceived
                .Subscribe(key => MelodyPlayer.Instance.RenderLoopKey(key))
                .AddTo(this);

            _gateway.PlaybackBatonReceived
                .Subscribe(key => MelodyPlayer.Instance.ReceiveBaton(key))
                .AddTo(this);
        }

        /// <summary>
        /// 演奏の事実（周キー・バトン）の送信。発生源はその時のトークン保持者（音源側）で
        /// 先生・生徒どちらにもなり得るため、役割で分岐せずどちらの端末も同じ配線にする。
        /// エコー防止は由来分離で担保される：これらのストリームにはローカル発イベントしか
        /// 流れず、受信は上の ApplyRemote / RenderLoopKey / ReceiveBaton に直接届く。
        /// </summary>
        private void SubscribePerformanceSends()
        {
            MelodyPlayer.Instance.OnLoopKeyPlayed
                .Subscribe(_gateway.SendLoopKey)
                .AddTo(this);

            MelodyPlayer.Instance.OnBatonPassed
                .Subscribe(_gateway.SendPlaybackBaton)
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

            // 自動キー変更は先生の「意図」。ローカル操作由来の変更だけを送る（受信の再送信をしない）。
            AutoKeyChangeManager.Instance.LocalChanged
                .Subscribe(_gateway.SendAutoKeyChange)
                .AddTo(this);

            if (SoundSourceSwitcher.Instance != null)
            {
                SoundSourceSwitcher.Instance.OnChanged
                    .Subscribe(_gateway.SendSoundSetSelection)
                    .AddTo(this);
            }
        }

        /// <summary>
        /// ルームが実質的に終わったとき（自分の退室・切断、または先生の退室）に、
        /// ルーム由来の状態をすべて既定へ戻す（生徒のみ）。
        /// 自動再生ループの条件はローカル状態だけを見るため、ここで戻さないと
        /// 相手がいなくなっても再生が回り続け、単独演奏時にも古い状態でループが再発する。
        /// 再入室時の復元は先生側の ResendCurrentState が行う。
        /// </summary>
        private void SubscribeRoomStateReset()
        {
            _gateway.SelfLeftRoom
                .Merge(_gateway.TeacherLeftRoom)
                .Subscribe(_ => ResetRoomDerivedState())
                .AddTo(this);
        }

        private static void ResetRoomDerivedState()
        {
            MelodyPlayer.Instance.FinishMelody();
            AutoKeyChangeManager.Instance.ApplyRemote(AutoKeyChangeState.None);
            SoundPlayManager.Instance.SetState(false);
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
            EarphoneModeManager.Instance.SetStudentConnected(_gateway.HasConnectedStudent);

            // 生徒ありに切り替えると、実効状態が先生の意図（トグル）に戻る。
            _gateway.StudentPresenceChanged
                .Subscribe(connected => SoundPlayManager.Instance.SetStudentConnected(connected))
                .AddTo(this);

            _gateway.StudentPresenceChanged
                .Subscribe(connected => EarphoneModeManager.Instance.SetStudentConnected(connected))
                .AddTo(this);

            // 再生権限（バトン）を持つ生徒が切断すると委譲が永遠に完了しないため、先生が自己回収する。
            // 上の SetStudentConnected の購読が先に走り、実効状態（先生が音源側）へ戻ってから呼ばれる。
            _gateway.StudentPresenceChanged
                .Where(connected => !connected)
                .Subscribe(_ => MelodyPlayer.Instance.ReclaimPlaybackAuthority())
                .AddTo(this);

            _gateway.StudentJoined
                .Subscribe(_ => ResendCurrentState())
                .AddTo(this);
        }

        /// <summary>
        /// 後から入室した生徒に、先生の現在の状態（選択メロディ・音再生状態・BPM・自動キー変更・音源）を再送する。
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
            _gateway.SendAutoKeyChange(AutoKeyChangeManager.Instance.Current);

            if (SoundSourceSwitcher.Instance != null)
            {
                _gateway.SendSoundSetSelection(SoundSourceSwitcher.Instance.CurrentIndex);
            }
        }
    }
}
