using System;
using CubeTower.Application.Save;
using CubeTower.Core.Save;
using CubeTower.Infrastructure.Save;
using CubeTower.Infrastructure.Save.Crypto;
using Zenject;

namespace CubeTower.Composition.Installers
{
    public static class SaveInstaller
    {
        public static void Install(DiContainer container, bool encryptSaves, float autoSaveDelaySeconds)
        {
            container.BindInterfacesTo<GameRepository>().AsSingle();

            if (encryptSaves)
            {
                container.BindInterfacesTo<AesHmacSaveDataProtector>().AsSingle()
                    .WithArguments(UnityEngine.Application.identifier);
            }
            else
            {
                container.Bind<ISaveDataProtector>().To<PlainSaveDataProtector>().AsSingle();
            }

            container.Bind<IGameStateSaver>().To<JsonFileGameStateSaver>().AsSingle()
                .WithArguments(SaveFileLocation.DefaultPath);

            container.Bind<ISaveLoader>().To<TowerSaveLoader>().AsSingle();

            container.Bind<ISaveLoadManager>().To<SaveLoadManager>().AsSingle();
            container.BindInterfacesAndSelfTo<AutoSaveService>().AsSingle()
                .WithArguments(TimeSpan.FromSeconds(autoSaveDelaySeconds));
        }
    }
}
