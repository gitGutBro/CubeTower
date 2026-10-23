namespace CubeTower.Application.StateMachine
{
    /// <summary>
    /// Creates states. Implemented in the composition root with the DI container,
    /// so the state machine itself does not depend on Zenject.
    /// </summary>
    public interface IStateFactory
    {
        TState GetState<TState>() where TState : class, IExitableState;
    }
}
