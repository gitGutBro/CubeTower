using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CubeTower.Application.Config;
using CubeTower.Application.Lifecycle;
using CubeTower.Application.Localization;
using CubeTower.Application.Save;
using CubeTower.Application.Services;
using CubeTower.Application.StateMachine;
using CubeTower.Core.Common;
using Cysharp.Threading.Tasks;

namespace CubeTower.Application.States
{
    /// <summary>
    /// Prepares everything the game needs before any scene is shown:
    /// loads and validates the config, initializes async services (encryption keys),
    /// loads the save. Then switches to loading the game scene.
    /// </summary>
    public sealed class BootstrapState : IState, IDisposable
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly ILoadingCurtain _curtain;
        private readonly IGameConfigProvider _configProvider;
        private readonly GameConfigStore _configStore;
        private readonly IReadOnlyList<IAsyncInitializable> _asyncInitializables;
        private readonly ISaveLoadManager _saveLoadManager;
        private readonly ILocalizationService _localization;
        private readonly ILog _log;

        private CancellationTokenSource _cancellation;

        public BootstrapState(
            IGameStateMachine stateMachine,
            ILoadingCurtain curtain,
            IGameConfigProvider configProvider,
            GameConfigStore configStore,
            IEnumerable<IAsyncInitializable> asyncInitializables,
            ISaveLoadManager saveLoadManager,
            ILocalizationService localization,
            ILog log)
        {
            _stateMachine = stateMachine;
            _curtain = curtain;
            _configProvider = configProvider;
            _configStore = configStore;
            _asyncInitializables = asyncInitializables.ToArray();
            _saveLoadManager = saveLoadManager;
            _localization = localization;
            _log = log;
        }

        public void Enter()
        {
            _curtain.Show(_localization.Get(LocalizationKeys.Loading));

            _cancellation = new CancellationTokenSource();
            RunAsync(_cancellation.Token).Forget();
        }

        public void Exit() => 
            CancelRun();

        public void Dispose() => 
            CancelRun();

        private async UniTaskVoid RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                UniTask initialization = UniTask.WhenAll(
                    _asyncInitializables.Select(service => service.InitializeAsync(cancellationToken)));

                GameConfig config = await _configProvider.LoadAsync(cancellationToken);
                await initialization;

                ConfigValidationReport report = GameConfigValidator.Validate(config);
                
                foreach (string warning in report.Warnings)
                    _log.Warning($"Config: {warning}");

                if (!report.IsValid)
                {
                    _log.Error("Config is invalid: " + string.Join(" ", report.Errors));
                    _curtain.ShowError(_localization.Get(LocalizationKeys.StartupError));
                    return;
                }

                _configStore.Set(config);
                await _saveLoadManager.LoadGameAsync(cancellationToken);

                _stateMachine.Enter<LoadLevelState, string>(SceneAddress.Game);
            }
            catch (OperationCanceledException) { }
            catch (Exception exception)
            {
                _log.Error("Game startup failed.", exception);
                _curtain.ShowError(_localization.Get(LocalizationKeys.StartupError));
            }
        }

        private void CancelRun()
        {
            if (_cancellation == null)
                return;

            _cancellation.Cancel();
            _cancellation.Dispose();
            _cancellation = null;
        }
    }
}
