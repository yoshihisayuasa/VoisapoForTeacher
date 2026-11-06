using System;

namespace Assets.Scripts.Domain.ValueObjects
{
    public sealed class AppVersion : ValueObject<AppVersion>
    {
        private readonly Version _version;

        public AppVersion(string versionString)
        {
            _version = Parse(versionString);
        }

        private static Version Parse(string versionString)
        {
            if (!Version.TryParse(versionString, out var parsed))
            {
                throw new ArgumentException($"無効なバージョン形式: {versionString}");
            }
            return parsed;
        }

        public static bool TryCreate(string versionString, out AppVersion result)
        {
            if (Version.TryParse(versionString, out _))
            {
                result = new AppVersion(versionString);
                return true;
            }
            result = null;
            return false;
        }

        public bool IsNewerThan(AppVersion other) => _version.CompareTo(other._version) > 0;

        public override string ToString() => _version.ToString();

        protected override bool EqualsCore(AppVersion other) => _version == other._version;

        protected override int GetHashCodeCore() => _version.GetHashCode();
    }
}
