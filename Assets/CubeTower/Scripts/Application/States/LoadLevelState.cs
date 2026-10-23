using System;
using System.Threading;
using CubeTower.Application.Localization;
using CubeTower.Application.Services;
using CubeTower.Application.StateMachine;
using CubeTower.Core.Common;
using Cysharp.Threading.Tasks;

namespace CubeTower.Application.States
{
    /// <summary>
    /// Loads a scene asynchronously and enters the gameplay state when it is ready.
    /// The course version used a coroutine runner; UniTask runs on the player loop
    /// and does not die together with the unloaded bootstrap scene.
    /// </summary>
    public sealed class LoadLevelState : IPayloadedState<string>, IDisposable
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly ISceneLoader _sceneLoader;
        private readonly ILoadingCurtain _curtain;
        private readonly ILocalizationService _localization;
        private readonly ILog _log;

        private CancellationTokenSource _cancellation;

        public LoadLevelState(
            IGameStateMachine stateMachine,
            ISceneLoader sceneLoader,
            ILoadingCurtain curtain,
            ILocalizationService localization,
            ILog log)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
            _localization = localization;
            _log = log;
        }

        public void Enter(string sceneName)
        {
            _curtain.Show(_localization.Get(LocalizationKeys.Loading));

            _cancellation = new CancellationTokenSource();
            LoadAsync(sceneName, _cancellation.Token).Forget();
        }

        public void Exit() => 
            CancelLoad();

        public void Dispose() => 
            CancelLoad();

        private async UniTaskVoid LoadAsync(string sceneName, CancellationToken cancellationToken)
        {
            try
            {
                await _sceneLoader.LoadAsync(sceneName, cancellationToken);
                _stateMachine.Enter<LevelLoopState>();
            }
            catch (OperationCanceledException) { }
            catch (Exception exception)
            {
                _log.Error($"Failed to load scene '{sceneName}'.", exception);
                _curtain.ShowError(_localization.Get(LocalizationKeys.StartupError));
            }
        }

        private void CancelLoad()
        {
            if (_cancellation == null)
                return;

            _cancellation.Cancel();
            _cancellation.Dispose();
            _cancellation = null;
        }
    }
}
