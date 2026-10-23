using CubeTower.Application.Services;
using CubeTower.Application.StateMachine;
using CubeTower.Application.States;
using CubeTower.Composition.Boot;
using CubeTower.Presentation.Views;
using Zenject;

namespace CubeTower.Composition.Installers
{
    public static class StateMachineInstaller
    {
        public static void Install(DiContainer container, LoadingCurtainView curtainPrefab)
        {
            container.BindInterfacesTo<LoadingCurtainView>().FromComponentInNewPrefab(curtainPrefab).AsSingle();
            container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();

            container.Bind<IStateFactory>().To<StateFactory>().AsSingle();
            container.Bind<IGameStateMachine>().To<GameStateMachine>().AsSingle();

            container.BindInterfacesAndSelfTo<BootstrapState>().AsSingle();
            container.BindInterfacesAndSelfTo<LoadLevelState>().AsSingle();
            container.BindInterfacesAndSelfTo<LevelLoopState>().AsSingle();
        }
    }
}
