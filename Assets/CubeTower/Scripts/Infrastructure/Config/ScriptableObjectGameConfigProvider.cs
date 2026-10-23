using System;
using System.Threading;
using CubeTower.Application.Config;
using Cysharp.Threading.Tasks;

namespace CubeTower.Infrastructure.Config
{
    /// <summary>
    /// The only config source required by the task. Returns synchronously,
    /// but keeps the async contract so a file or server provider can replace it later.
    /// </summary>
    public sealed class ScriptableObjectGameConfigProvider : IGameConfigProvider
    {
        private readonly GameConfigAsset _asset;

        public ScriptableObjectGameConfigProvider(GameConfigAsset asset) => 
            _asset = asset;

        public UniTask<GameConfig> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_asset == null)
                throw new InvalidOperationException(
                    "GameConfigAsset is not assigned in the ProjectInstaller. Run Tools/CubeTower/Build Scene.");

            return UniTask.FromResult(_asset.ToConfig());
        }
    }
}
