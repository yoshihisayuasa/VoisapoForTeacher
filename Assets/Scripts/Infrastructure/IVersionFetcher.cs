using AsseScripts.Domain;
using Cysharp.Threading.Tasks;

namespace AsseScripts.Infrastructure
{
    public interface IVersionFetcher
    {
        UniTask<VersionCheckResult> FetchLatestAsync();
    }
}
