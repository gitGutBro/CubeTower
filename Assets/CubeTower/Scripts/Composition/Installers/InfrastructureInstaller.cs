using CubeTower.Application.Config;
using CubeTower.Application.Localization;
using CubeTower.Core.Common;
using CubeTower.Infrastructure.Config;
using CubeTower.Infrastructure.Diagnostics;
using CubeTower.Infrastructure.Lifecycle;
using CubeTower.Infrastructure.Localization;
using CubeTower.Infrastructure.Logging;
using Zenject;

namespace CubeTower.Composition.Installers
{
    public static class InfrastructureInstaller
    {
        public static void Install(DiContainer container, GameConfigAsset gameConfig, LocalizationTableAsset localizationTable)
        {
            container.Bind<ILog>().To<UnityLog>().AsSingle();
            
            container.BindInterfacesAndSelfTo<GlobalExceptionHandlers>().AsSingle();
            container.BindInterfacesTo<UnityApplicationLifecycle>().AsSingle();

            container.Bind<IGameConfigProvider>().To<ScriptableObjectGameConfigProvider>().AsSingle().WithArguments(gameConfig);
            container.BindInterfacesAndSelfTo<GameConfigStore>().AsSingle();

            container.Bind<ILocalizationService>().To<TableLocalizationService>().AsSingle().WithArguments(localizationTable);
        }
    }
}
