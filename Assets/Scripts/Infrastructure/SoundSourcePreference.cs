using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    /// <summary>
    /// 選択中の音源セットを端末に覚えさせる／思い出す。
    /// </summary>
    public static class SoundSourcePreference
    {
        private const string PrefsKey = "SoundSourceIndex";

        public static void Remember(int index)
        {
            PlayerPrefs.SetInt(PrefsKey, index);
            PlayerPrefs.Save();
        }

        public static int Recall(int defaultIndex)
        {
            return PlayerPrefs.GetInt(PrefsKey, defaultIndex);
        }
    }
}
