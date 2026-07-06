namespace Assets.Scripts.Domain.ValueObjects
{
    public sealed class VersionCheckResult
    {
        public bool HasUpdate { get; }
        public bool IsError { get; }
        public AppVersion LatestVersion { get; }
        public string DownloadUrl { get; }

        private VersionCheckResult(bool hasUpdate, bool isError, AppVersion latestVersion, string downloadUrl)
        {
            HasUpdate = hasUpdate;
            IsError = isError;
            LatestVersion = latestVersion;
            DownloadUrl = downloadUrl;
        }

        public static VersionCheckResult UpdateAvailable(AppVersion latest, string downloadUrl)
            => new(true, false, latest, downloadUrl);

        public static VersionCheckResult UpToDate()
            => new(false, false, null, null);

        public static VersionCheckResult FetchFailed()
            => new(false, true, null, null);
    }
}
