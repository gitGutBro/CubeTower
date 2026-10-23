using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace CubeTower.Presentation.Tweening
{
    /// <summary>
    /// Awaits a DOTween tween with cancellation.
    /// UniTask has a built-in DOTween integration, but it needs the UNITASK_DOTWEEN_SUPPORT define
    /// that DOTween from the Asset Store does not set. This small bridge needs no setup.
    /// <para>
    /// Semantics: completes when the tween completes; throws <see cref="System.OperationCanceledException"/>
    /// when the token is cancelled (the tween is killed) or when the tween is killed from outside
    /// (e.g. its GameObject is destroyed via SetLink). So an awaiting presenter never mistakes
    /// an interrupted animation for a finished one.
    /// </para>
    /// </summary>
    public static class TweenAwaitExtensions
    {
        public static UniTask AwaitCompletionAsync(this Tween tween, CancellationToken cancellationToken)
        {
            if (tween == null || !tween.IsActive())
                return UniTask.CompletedTask;

            if (cancellationToken.IsCancellationRequested)
            {
                tween.Kill();
                return UniTask.FromCanceled(cancellationToken);
            }

            UniTaskCompletionSource completion = new();
            bool isCompleted = false;
            CancellationTokenRegistration registration = default;

            tween.OnComplete(() => isCompleted = true);
            tween.OnKill(() =>
            {
                registration.Dispose();

                if (isCompleted)
                    completion.TrySetResult();
                else
                    completion.TrySetCanceled(cancellationToken);
            });

            registration = cancellationToken.Register(() => tween.Kill());
            return completion.Task;
        }
    }
}
