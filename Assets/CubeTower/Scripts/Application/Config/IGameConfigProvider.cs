using System.Threading;
using Cysharp.Threading.Tasks;

namespace CubeTower.Application.Config
{
    /// <summary>
    /// Source of the game configuration. Async on purpose: a future provider may read a file
    /// or a server. The only implementation in the game reads a ScriptableObject.
    /// </summary>
    public interface IGameConfigProvider
    {
        UniTask<GameConfig> LoadAsync(CancellationToken cancellationToken);
    }
}
