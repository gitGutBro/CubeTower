using System;
using CubeTower.Application.Localization;
using CubeTower.Core.Tower;
using CubeTower.Presentation.Views;
using R3;

namespace CubeTower.Presentation.Presenters
{
    /// <summary>
    /// Comments every action with a text above the bottom part of the screen:
    /// placing, throwing out, disappearing of a cube, the height limit.
    /// Listens to the tower model and to drag-and-drop outcomes; texts come from localization.
    /// </summary>
    public sealed class MessagePresenter : IDisposable
    {
        private readonly IReadOnlyTower _tower;
        private readonly IDragDropEvents _dragDrop;
        private readonly MessageView _view;
        private readonly ILocalizationService _localization;
        private readonly CompositeDisposable _subscriptions = new();

        public MessagePresenter(
            IReadOnlyTower tower,
            IDragDropEvents dragDrop,
            MessageView view,
            ILocalizationService localization)
        {
            _tower = tower;
            _dragDrop = dragDrop;
            _view = view;
            _localization = localization;
        }

        public void Initialize()
        {
            _tower.Added.Subscribe(_ => Show(LocalizationKeys.CubePlaced)).AddTo(_subscriptions);
            _tower.Removed.Subscribe(_ => Show(LocalizationKeys.CubeRemoved)).AddTo(_subscriptions);
            _dragDrop.Missed.Subscribe(_ => Show(LocalizationKeys.CubeMissed)).AddTo(_subscriptions);
            _dragDrop.Returned.Subscribe(_ => Show(LocalizationKeys.CubeReturned)).AddTo(_subscriptions);
            _dragDrop.HeightLimitReached.Subscribe(_ => Show(LocalizationKeys.TowerFull)).AddTo(_subscriptions);
            _dragDrop.Rejected.Subscribe(reason => Show(ToMessageKey(reason))).AddTo(_subscriptions);
        }

        public void Dispose() => 
            _subscriptions.Dispose();

        private static string ToMessageKey(string failReason)
        {
            switch (failReason)
            {
                case PlacementFailReasons.MissedTower:
                    return LocalizationKeys.CubeMissed;
                
                case PlacementFailReasons.HeightLimit:
                    return LocalizationKeys.HeightLimit;
                
                case PlacementFailReasons.ColorMismatch:
                    return LocalizationKeys.ColorMismatch;
                
                default:
                    return LocalizationKeys.PlacementRejected;
            }
        }

        private void Show(string key) => 
            _view.Show(_localization.Get(key));
    }
}
