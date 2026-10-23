using CubeTower.Core.Tower;
using CubeTower.Presentation.Layout;
using CubeTower.Presentation.Presenters;
using CubeTower.Presentation.Tweening;
using CubeTower.Presentation.Views;
using UnityEngine;
using Zenject;

namespace CubeTower.Composition.Boot
{
    /// <summary>
    /// Entry point of the game scene. Presenters do not implement Zenject's IInitializable:
    /// the order of their start is explicit here instead of being hidden in container internals.
    /// </summary>
    public sealed class GameEntryPoint : IInitializable
    {
        private readonly ITower _tower;
        private readonly TowerLayout _layout;
        private readonly MessageView _messageView;
        private readonly FadeTweener _fadeTweener;
        private readonly PalettePresenter _palettePresenter;
        private readonly TowerPresenter _towerPresenter;
        private readonly DragDropPresenter _dragDropPresenter;
        private readonly MessagePresenter _messagePresenter;

        public GameEntryPoint(
            ITower tower,
            TowerLayout layout,
            MessageView messageView,
            FadeTweener fadeTweener,
            PalettePresenter palettePresenter,
            TowerPresenter towerPresenter,
            DragDropPresenter dragDropPresenter,
            MessagePresenter messagePresenter)
        {
            _tower = tower;
            _layout = layout;
            _messageView = messageView;
            _fadeTweener = fadeTweener;
            _palettePresenter = palettePresenter;
            _towerPresenter = towerPresenter;
            _dragDropPresenter = dragDropPresenter;
            _messagePresenter = messagePresenter;
        }

        public void Initialize()
        {
            Canvas.ForceUpdateCanvases();

            _messageView.Initialize(_fadeTweener);

            _palettePresenter.Initialize();
            _towerPresenter.Initialize();
            _dragDropPresenter.Initialize();
            _messagePresenter.Initialize();
            
            _tower.FitToArea(_layout.AreaWidth, _layout.AreaHeight);
        }
    }
}
