using System.Threading;
using Cysharp.Threading.Tasks;

namespace CubeTower.Application.Services
{
    public interface ISceneLoader
    {
        /// <summary>Loads a scene and starts its DI context. Completes when the scene is ready.</summary>
        UniTask LoadAsync(string sceneName, CancellationToken cancellationToken);
    }
}
