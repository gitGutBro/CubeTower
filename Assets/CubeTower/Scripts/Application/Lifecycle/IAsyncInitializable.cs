using System.Threading;
using Cysharp.Threading.Tasks;

namespace CubeTower.Application.Lifecycle
{
    /// <summary>
    /// A service that needs asynchronous preparation before the game starts
    /// (for example, deriving encryption keys on a background thread).
    /// All such services are awaited in the bootstrap state.
    /// </summary>
    public interface IAsyncInitializable
    {
        UniTask InitializeAsync(CancellationToken cancellationToken);
    }
}
