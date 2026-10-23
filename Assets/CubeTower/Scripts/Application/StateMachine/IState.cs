namespace CubeTower.Application.StateMachine
{
    /// <summary>A state that can be left. Base of all game states.</summary>
    public interface IExitableState
    {
        void Exit();
    }

    /// <summary>A state entered without arguments.</summary>
    public interface IState : IExitableState
    {
        void Enter();
    }

    /// <summary>A state that needs data to start, e.g. the name of the scene to load.</summary>
    public interface IPayloadedState<in TPayload> : IExitableState
    {
        void Enter(TPayload payload);
    }
}
