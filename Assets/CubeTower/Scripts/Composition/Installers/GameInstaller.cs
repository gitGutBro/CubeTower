using CubeTower.Composition.Boot;
using CubeTower.Presentation.Factories;
using CubeTower.Presentation.Input;
using CubeTower.Presentation.Layout;
using CubeTower.Presentation.Presenters;
using CubeTower.Presentation.Settings;
using CubeTower.Presentation.Tweening;
using CubeTower.Presentation.Views;
using UnityEngine;
using Zenject;

namespace CubeTower.Composition.Installers
{
    /// <summary>
    /// Installer of the game scene: views (scene objects) and presenters.
    /// The model and services come from the parent ProjectContext container.
    /// </summary>
    public sealed class GameInstaller : MonoInstaller
    {
        [Header("Scene views")]
        [SerializeField] private PaletteView _paletteView;
        [SerializeField] private TowerZoneView _towerZoneView;
        [SerializeField] private HoleView _holeView;
        [SerializeField] private MessageView _messageView;
        [SerializeField] private DragLayerView _dragLayerView;

        [Header("Assets")]
        [SerializeField] private CubeView _cubePrefab;
        [SerializeField] private AnimationSettings _animationSettings;
        [SerializeField] private PresentationSettings _presentationSettings;

        public override void InstallBindings()
        {
            BindViews();
            BindServices();
            BindPresenters();
        }

        private void BindViews()
        {
            Container.BindInstance(_paletteView);
            Container.BindInstance(_towerZoneView);
            Container.BindInstance(_holeView);
            Container.BindInstance(_messageView);
            Container.BindInstance(_dragLayerView);
        }

        private void BindServices()
        {
            Container.BindInstance(_animationSettings);
            Container.BindInstance(_presentationSettings);

            Container.Bind<CubeTweener>().AsSingle();
            Container.Bind<FadeTweener>().AsSingle();
            Container.Bind<CubeMetrics>().AsSingle();
            Container.Bind<TowerLayout>().AsSingle();
            Container.Bind<GestureClassifier>().AsSingle().WithArguments(_presentationSettings.ScrollMaxAngle);
            Container.Bind<IDragSession>().To<DragSession>().AsSingle();
            Container.Bind<ICubeViewFactory>().To<CubeViewFactory>().AsSingle()
                .WithArguments(_cubePrefab, _dragLayerView.PoolRoot);
        }

        private void BindPresenters()
        {
            Container.BindInterfacesAndSelfTo<PalettePresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<TowerPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<DragDropPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<MessagePresenter>().AsSingle();

            Container.BindInterfacesTo<GameEntryPoint>().AsSingle();
        }
    }
}
