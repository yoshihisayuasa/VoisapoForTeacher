namespace Assets.Scripts
{
    /// <summary>
    /// 起動中のビルドが先生用か生徒用かを表す。
    /// ビルドごとの Scripting Define Symbol `STUDENT` をここ1箇所だけで読み取り、
    /// アプリ全体へ単一の判定として提供する（#if をコード中に散らさないため）。
    /// </summary>
    public static class AppMode
    {
        public static bool IsTeacher =>
#if TEACHER
            true;
#else
            false;
#endif

    }
}
