using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    public sealed class VersionCheckResult : ValueObject<VersionCheckResult>
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

        protected override bool EqualsCore(VersionCheckResult other)
        {
            return HasUpdate == other.HasUpdate
                && IsError == other.IsError
                && LatestVersion == other.LatestVersion
                && DownloadUrl == other.DownloadUrl;
        }

        protected override int GetHashCodeCore()
        {
            return HashCode.Combine(HasUpdate, IsError, LatestVersion, DownloadUrl);
        }
    }
}
