using CubeTower.Core.Common;
using CubeTower.Core.Tower;
using CubeTower.Core.Tower.Rules;
using Zenject;

namespace CubeTower.Composition.Installers
{
    public static class TowerInstaller
    {
        public static void Install(DiContainer container)
        {
            container.Bind<IRandomService>().To<SystemRandomService>().AsSingle();
            container.Bind<IIdGenerator>().To<GuidIdGenerator>().AsSingle();

            container.Bind<IPlacementRule>().To<DropOverTowerRule>().AsSingle();
            container.Bind<IPlacementRule>().To<HeightLimitRule>().AsSingle();

            container.BindInterfacesTo<Tower>().AsSingle();
        }
    }
}
