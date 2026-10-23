using System;
using CubeTower.Application.Lifecycle;
using R3;

namespace CubeTower.Infrastructure.Lifecycle
{
    /// <summary>
    /// Turns the static events <c>Application.focusChanged</c> and <c>Application.quitting</c>
    /// into streams. Unsubscribes in Dispose: a static event keeps its subscribers alive,
    /// so forgetting to unsubscribe is a guaranteed leak (especially with Domain Reload disabled).
    /// </summary>
    public sealed class UnityApplicationLifecycle : IApplicationLifecycle, IDisposable
    {
        private readonly Subject<bool> _focusChanged = new();
        private readonly Subject<Unit> _quitting = new();
        private bool _isDisposed;

        public UnityApplicationLifecycle()
        {
            UnityEngine.Application.focusChanged += OnFocusChanged;
            UnityEngine.Application.quitting += OnQuitting;
        }

        public Observable<bool> FocusChanged => _focusChanged;
        public Observable<Unit> Quitting => _quitting;

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            
            UnityEngine.Application.focusChanged -= OnFocusChanged;
            UnityEngine.Application.quitting -= OnQuitting;
            _focusChanged.Dispose();
            _quitting.Dispose();
        }

        private void OnFocusChanged(bool hasFocus) => 
            _focusChanged.OnNext(hasFocus);

        private void OnQuitting() => 
            _quitting.OnNext(Unit.Default);
    }
}
