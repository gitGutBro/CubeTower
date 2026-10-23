using System;
using System.Threading;
using CubeTower.Application.Lifecycle;
using CubeTower.Core.Tower;
using Cysharp.Threading.Tasks;
using R3;

namespace CubeTower.Application.Save
{
    /// <summary>
    /// Decides WHEN to save: after the tower changes (debounced, so a burst of changes
    /// produces one write) and immediately when the app loses focus or quits.
    /// Debounce uses the Unity time provider, so everything stays on the main thread.
    /// </summary>
    public sealed class AutoSaveService : IDisposable
    {
        private readonly ISaveLoadManager _saveLoadManager;
        private readonly IReadOnlyTower _tower;
        private readonly IApplicationLifecycle _lifecycle;
        private readonly TimeSpan _debounce;
        private readonly CancellationTokenSource _lifetime = new();

        private IDisposable _subscriptions;
        private bool _isDisposed;

        public AutoSaveService(
            ISaveLoadManager saveLoadManager,
            IReadOnlyTower tower,
            IApplicationLifecycle lifecycle,
            TimeSpan debounce)
        {
            _saveLoadManager = saveLoadManager ?? throw new ArgumentNullException(nameof(saveLoadManager));
            _tower = tower ?? throw new ArgumentNullException(nameof(tower));
            _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
            _debounce = debounce;
        }

        private bool IsRunning => _subscriptions != null;

        public void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(AutoSaveService));

            if (IsRunning)
                return;

            IDisposable changes = Observable
                .Merge(_tower.Added.AsUnitObservable(), _tower.Removed.AsUnitObservable(), _tower.Reset)
                .Debounce(_debounce, UnityTimeProvider.Update)
                .Subscribe(_ => _saveLoadManager.SaveGameAsync(_lifetime.Token).Forget());

            IDisposable focus = _lifecycle.FocusChanged
                .Where(hasFocus => !hasFocus)
                .Subscribe(_ => _saveLoadManager.SaveGameImmediate());

            IDisposable quit = _lifecycle.Quitting
                .Subscribe(_ => _saveLoadManager.SaveGameImmediate());

            _subscriptions = Disposable.Combine(changes, focus, quit);
        }

        public void Stop()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            
            Stop();
            _lifetime.Cancel();
            _lifetime.Dispose();
        }
    }
}
