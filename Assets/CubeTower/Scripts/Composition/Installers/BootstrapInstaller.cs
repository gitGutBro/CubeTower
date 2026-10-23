using CubeTower.Composition.Boot;
using Zenject;

namespace CubeTower.Composition.Installers
{
    /// <summary>
    /// Installer of the bootstrap scene. Global services are already in the ProjectContext;
    /// the bootstrap scene only starts the application.
    /// </summary>
    public sealed class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings() => 
            Container.BindInterfacesTo<AppInitializer>().AsSingle();
    }
}
