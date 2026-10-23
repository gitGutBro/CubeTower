using System.Threading;
using Cysharp.Threading.Tasks;

namespace CubeTower.Application.Save
{
    public interface ISaveLoadManager
    {
        UniTask LoadGameAsync(CancellationToken cancellationToken);
        UniTask SaveGameAsync(CancellationToken cancellationToken);
        void SaveGameImmediate();
    }
}
