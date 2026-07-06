using Assets.Scripts.Domain.ValueObjects;
using Assets.Scripts.UI;
using System.Collections.Generic;

namespace Assets.Scripts.UI.Piano
{
    /// <summary>
    /// 役割：鍵盤の再生・停止と「再生中／演奏済み色」の状態管理。
    /// </summary>
    public sealed class PianoKeyPlayback
    {
        private readonly IReadOnlyDictionary<PianoNoteEnum, PianoKeyUI> _keys;
        private readonly HashSet<PianoNote> _playingKeys = new();
        private readonly HashSet<PianoNote> _coloredKeys = new();

        public PianoKeyPlayback(IReadOnlyDictionary<PianoNoteEnum, PianoKeyUI> keys)
        {
            _keys = keys;
        }

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

        /// <summary>
        /// 指定鍵盤の音だけを鳴らす（視覚変化なし）。生徒が鍵盤を押している間の単音演奏用。
        /// </summary>
        public void PlayKeySound(PianoNote note)
        {
            GetKeyUI(note).PlaySound(VolumeManager.Instance.Volume * SoundSourceSwitcher.Instance.CurrentVolumeMultiplier);
        }

        /// <summary>
        /// 指定鍵盤の音だけをフェードアウト停止する（視覚変化なし）。生徒が鍵盤を離したとき用。
        /// </summary>
        public void StopKeySound(PianoNote note)
        {
            GetKeyUI(note).StopSound(BPMManager.Instance.KeyFadeOutSeconds);
        }

        /// <summary>
        /// 鍵盤を停止し、視覚をデフォルト色に戻す。
        /// </summary>
        public void Stop(PianoNote key)
        {
            StopCore(key, setPlayedColor: false);
        }

        /// <summary>
        /// 鍵盤を停止し、「演奏済み」色を付けて残す。
        /// </summary>
        public void StopAndMarkPlayed(PianoNote key)
        {
            StopCore(key, setPlayedColor: true);
        }

        private void StopCore(PianoNote key, bool setPlayedColor)
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
            keyUI.StopSound(BPMManager.Instance.KeyFadeOutSeconds);
            keyUI.SetKeyVisual(setPlayedColor);
        }

        /// <summary>
        /// 再生中の全鍵盤を停止する。
        /// setKeyVisual が true なら「演奏済み」色も含めて全色をリセットし、
        /// false なら再生中だった鍵盤を「演奏済み」色として保持する。
        /// </summary>
        public void StopAllKeys(bool setKeyVisual)
        {
            float fadeOut = BPMManager.Instance.KeyFadeOutSeconds;
            foreach (var key in _playingKeys)
            {
                var keyUI = GetKeyUI(key);
                keyUI.StopSound(fadeOut);
                if (setKeyVisual)
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
                    GetKeyUI(key).SetKeyVisual(false);
                }
                _coloredKeys.Clear();
            }
        }

        private PianoKeyUI GetKeyUI(PianoNote key)
        {
            return _keys[key.Note];
        }
    }
}
