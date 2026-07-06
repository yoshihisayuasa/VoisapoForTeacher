using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    public sealed class AppVersion : ValueObject<AppVersion>
    {
        private readonly Version _version;

        public AppVersion(string versionString)
        {
            if (!Version.TryParse(versionString, out var parsed))
                throw new ArgumentException($"無効なバージョン形式: {versionString}");
            _version = parsed;
        }

        private AppVersion(Version version)
        {
            _version = version;
        }

        public static bool TryCreate(string versionString, out AppVersion result)
        {
            if (Version.TryParse(versionString, out var parsed))
            {
                result = new AppVersion(parsed);
                return true;
            }
            result = null;
            return false;
        }

        public bool IsNewerThan(AppVersion other) => CompareTo(other) > 0;

        public override string ToString() => _version.ToString();

        public override int CompareTo(AppVersion other)
        {
            if (other is null) return 1;
            return _version.CompareTo(other._version);
        }

        protected override bool EqualsCore(AppVersion other) => _version == other._version;

        protected override int GetHashCodeCore() => _version.GetHashCode();
    }
}
