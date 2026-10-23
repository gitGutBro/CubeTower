using R3;

namespace CubeTower.Application.Lifecycle
{
    /// <summary>
    /// Application focus and quit events. Wraps static Unity events so that
    /// services can be tested and do not subscribe to statics directly.
    /// </summary>
    public interface IApplicationLifecycle
    {
        /// <summary>False when the app goes to background (on mobile it may be killed after that).</summary>
        Observable<bool> FocusChanged { get; }
        Observable<Unit> Quitting { get; }
    }
}
