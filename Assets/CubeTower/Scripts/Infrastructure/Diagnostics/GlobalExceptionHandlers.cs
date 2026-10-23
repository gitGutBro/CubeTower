using System;
using CubeTower.Core.Common;
using Cysharp.Threading.Tasks;
using R3;

namespace CubeTower.Infrastructure.Diagnostics
{
    /// <summary>
    /// Routes exceptions that nobody awaited or observed (fire-and-forget UniTasks,
    /// errors inside R3 subscribers) into the log, so they are never lost silently.
    /// Cancellation is a normal outcome and is not reported.
    /// </summary>
    public sealed class GlobalExceptionHandlers : IDisposable
    {
        private readonly ILog _log;
        private bool _isRegistered;

        public GlobalExceptionHandlers(ILog log) => 
            _log = log ?? throw new ArgumentNullException(nameof(log));

        public void Register()
        {
            if (_isRegistered)
                return;

            _isRegistered = true;
            UniTaskScheduler.PropagateOperationCanceledException = false;
            UniTaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            ObservableSystem.RegisterUnhandledExceptionHandler(OnUnhandledObservableException);
        }

        public void Dispose()
        {
            if (!_isRegistered)
                return;

            _isRegistered = false;
            UniTaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        }

        private void OnUnobservedTaskException(Exception exception)
        {
            if (exception is OperationCanceledException)
                return;

            _log.Error("Unobserved UniTask exception.", exception);
        }

        private void OnUnhandledObservableException(Exception exception) => 
            _log.Error("Unhandled exception in an R3 subscription.", exception);
    }
}
