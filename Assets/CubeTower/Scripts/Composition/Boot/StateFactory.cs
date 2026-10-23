using CubeTower.Application.StateMachine;
using Zenject;

namespace CubeTower.Composition.Boot
{
    /// <summary>
    /// Resolves states from the container. States are bound as singles,
    /// so their dependencies are injected by Zenject and the state machine stays DI-agnostic.
    /// </summary>
    public sealed class StateFactory : IStateFactory
    {
        private readonly DiContainer _container;

        public StateFactory(DiContainer container) => 
            _container = container;

        public TState GetState<TState>() where TState : class, IExitableState => 
            _container.Resolve<TState>();
    }
}
