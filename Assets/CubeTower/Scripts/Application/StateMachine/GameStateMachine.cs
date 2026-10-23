using System;

namespace CubeTower.Application.StateMachine
{
    /// <summary>
    /// Application-level state machine:
    /// Bootstrap -> LoadLevel(scene) -> LevelLoop.
    /// The course version also implemented ITickable to tick the current state;
    /// this game has no per-frame logic, so ticking is left out.
    /// </summary>
    public sealed class GameStateMachine : IGameStateMachine
    {
        private readonly IStateFactory _stateFactory;

        public GameStateMachine(IStateFactory stateFactory) => 
            _stateFactory = stateFactory ?? throw new ArgumentNullException(nameof(stateFactory));

        public IExitableState CurrentState { get; private set; }

        public void Enter<TState>() where TState : class, IState
        {
            TState state = ChangeState<TState>();
            state.Enter();
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
        {
            TState state = ChangeState<TState>();
            state.Enter(payload);
        }

        private TState ChangeState<TState>() where TState : class, IExitableState
        {
            CurrentState?.Exit();

            TState state = _stateFactory.GetState<TState>();
            CurrentState = state;
            return state;
        }
    }
}
