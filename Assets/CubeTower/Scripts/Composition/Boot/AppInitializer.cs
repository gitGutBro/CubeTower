using CubeTower.Application.StateMachine;
using CubeTower.Application.States;
using CubeTower.Infrastructure.Diagnostics;
using Zenject;

namespace CubeTower.Composition.Boot
{
    /// <summary>
    /// Entry point of the application. Zenject calls Initialize()
    /// right after the bootstrap scene container is built, so no MonoBehaviour Awake/Start is needed.
    /// The course version loaded the save here synchronously; our save is async (file IO and
    /// key derivation on background threads), so it is loaded inside the BootstrapState instead.
    /// </summary>
    public sealed class AppInitializer : IInitializable
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly GlobalExceptionHandlers _exceptionHandlers;

        public AppInitializer(IGameStateMachine stateMachine, GlobalExceptionHandlers exceptionHandlers)
        {
            _stateMachine = stateMachine;
            _exceptionHandlers = exceptionHandlers;
        }

        public void Initialize()
        {
            _exceptionHandlers.Register();
            BootstrapFlag.MarkBootstrapped();
            _stateMachine.Enter<BootstrapState>();
        }
    }
}
