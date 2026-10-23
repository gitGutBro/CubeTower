using System;
using System.Threading;
using CubeTower.Application.Config;
using CubeTower.Application.Lifecycle;
using CubeTower.Core.Tower;
using CubeTower.Presentation.Factories;
using CubeTower.Presentation.Input;
using CubeTower.Presentation.Layout;
using CubeTower.Presentation.Views;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CubeTower.Presentation.Presenters
{
    /// <summary>
    /// Drag and drop of cubes. A small explicit state machine of the gesture.
    /// The kind of the gesture is decided once at its start, so a scroll never turns into a drag.
    /// Only one pointer is tracked; other fingers are ignored until the gesture ends.
    /// Drop animations do not block input: the next drag may start immediately.
    /// </summary>
    public sealed class DragDropPresenter : IDragDropEvents, IDisposable
    {
        private readonly ITower _tower;
        private readonly PalettePresenter _palette;
        private readonly TowerPresenter _towerPresenter;
        private readonly PaletteView _paletteView;
        private readonly HoleView _hole;
        private readonly DragLayerView _dragLayer;
        private readonly TowerLayout _layout;
        private readonly CubeMetrics _metrics;
        private readonly ICubeViewFactory _factory;
        private readonly IGameConfigStore _config;
        private readonly IDragSession _dragSession;
        private readonly GestureClassifier _classifier;
        private readonly IApplicationLifecycle _lifecycle;

        private readonly Subject<Unit> _missed = new();
        private readonly Subject<string> _rejected = new();
        private readonly Subject<Unit> _returned = new();
        private readonly Subject<Unit> _heightLimitReached = new();
        private readonly CompositeDisposable _subscriptions = new();
        private readonly CancellationTokenSource _lifetime = new();

        private DragState _state = DragState.Idle;
        private int _pointerId;
        private PointerEventData _lastEventData;
        private CubeView _draggedView;
        private string _draggedColorId;
        private string _draggedCubeId;
        private bool _isDisposed;

        public DragDropPresenter(
            ITower tower,
            PalettePresenter palette,
            TowerPresenter towerPresenter,
            PaletteView paletteView,
            HoleView hole,
            DragLayerView dragLayer,
            TowerLayout layout,
            CubeMetrics metrics,
            ICubeViewFactory factory,
            IGameConfigStore config,
            IDragSession dragSession,
            GestureClassifier classifier,
            IApplicationLifecycle lifecycle)
        {
            _tower = tower;
            _palette = palette;
            _towerPresenter = towerPresenter;
            _paletteView = paletteView;
            _hole = hole;
            _dragLayer = dragLayer;
            _layout = layout;
            _metrics = metrics;
            _factory = factory;
            _config = config;
            _dragSession = dragSession;
            _classifier = classifier;
            _lifecycle = lifecycle;
        }

        private enum DragState
        {
            Idle,
            Scrolling,
            DraggingNew,
            DraggingTower,
        }

        public Observable<Unit> Missed => _missed;
        public Observable<string> Rejected => _rejected;
        public Observable<Unit> Returned => _returned;
        public Observable<Unit> HeightLimitReached => _heightLimitReached;

        public void Initialize()
        {
            _palette.CellDragEvent += OnPaletteDragEvent;
            _towerPresenter.CubeDragEvent += OnTowerDragEvent;

            _lifecycle.FocusChanged
                .Where(hasFocus => !hasFocus)
                .Subscribe(_ => Cancel())
                .AddTo(_subscriptions);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            
            _palette.CellDragEvent -= OnPaletteDragEvent;
            _towerPresenter.CubeDragEvent -= OnTowerDragEvent;
            _subscriptions.Dispose();
            _lifetime.Cancel();
            _lifetime.Dispose();
            _missed.Dispose();
            _rejected.Dispose();
            _returned.Dispose();
            _heightLimitReached.Dispose();
        }

        private void OnPaletteDragEvent(string colorId, CubeDragPhase phase, PointerEventData eventData)
        {
            switch (phase)
            {
                case CubeDragPhase.InitializePotentialDrag:
                    if (_state == DragState.Idle)
                        _paletteView.ForwardInitializePotentialDrag(eventData);
                    
                    break;

                case CubeDragPhase.Begin:
                    if (_state != DragState.Idle)
                        return;

                    StartPaletteGesture(colorId, eventData);
                    break;

                case CubeDragPhase.Drag:
                    if (!IsActivePointer(eventData))
                        return;

                    if (_state == DragState.Scrolling)
                        _paletteView.ForwardDrag(eventData);
                    else if (_state == DragState.DraggingNew)
                        FollowPointer(eventData);

                    break;

                case CubeDragPhase.End:
                    if (!IsActivePointer(eventData))
                        return;

                    if (_state == DragState.Scrolling)
                    {
                        _paletteView.ForwardEndDrag(eventData);
                        ResetGesture();
                    }
                    else if (_state == DragState.DraggingNew)
                    {
                        DropNewCube(eventData);
                    }

                    break;
            }
        }

        private void OnTowerDragEvent(string cubeId, CubeView view, CubeDragPhase phase, PointerEventData eventData)
        {
            switch (phase)
            {
                case CubeDragPhase.Begin:
                    if (_state != DragState.Idle)
                        return;

                    CubeView grabbed = _towerPresenter.BeginDrag(cubeId);

                    if (grabbed == null)
                        return;

                    BeginGesture(DragState.DraggingTower, eventData);
                    _draggedCubeId = cubeId;
                    _draggedView = grabbed;
                    _draggedView.SetRaycastTarget(false);
                    ScreenSpace.Reparent(_draggedView.Rect, _dragLayer.Layer);
                    FollowPointer(eventData);
                    break;

                case CubeDragPhase.Drag:
                    if (_state == DragState.DraggingTower && IsActivePointer(eventData))
                        FollowPointer(eventData);

                    break;

                case CubeDragPhase.End:
                    if (_state == DragState.DraggingTower && IsActivePointer(eventData))
                        DropTowerCube(eventData);

                    break;
            }
        }

        private void StartPaletteGesture(string colorId, PointerEventData eventData)
        {
            DragIntent intent = _classifier.Classify(eventData.position - eventData.pressPosition);

            if (intent == DragIntent.Scroll)
            {
                BeginGesture(DragState.Scrolling, eventData);
                _paletteView.ForwardBeginDrag(eventData);
                return;
            }

            if (!_config.TryGetDefinition(colorId, out CubeColorDefinition definition))
                return;

            BeginGesture(DragState.DraggingNew, eventData);
            _draggedColorId = colorId;
            _draggedView = _factory.Create(_dragLayer.Layer);
            _draggedView.SetAppearance(definition.Sprite, definition.Color);
            _draggedView.SetSize(_metrics.Size);
            _draggedView.SetRaycastTarget(false);
            FollowPointer(eventData);
        }

        private void DropNewCube(PointerEventData eventData)
        {
            CubeView view = _draggedView;
            string colorId = _draggedColorId;
            ResetGesture();

            if (!_layout.TryGetTowerPoint(eventData.position, eventData.pressEventCamera, out Vector2 point))
            {
                PlayExplosion(view);
                _missed.OnNext(Unit.Default);
                return;
            }

            _dragSession.Offer(view);
            PlacementResult result = _tower.TryPlace(
                new PlacementRequest(colorId, point.x, point.y, _layout.AreaWidth, _layout.AreaHeight));

            if (result.IsSuccess)
            {
                if (_tower.IsHeightLimitReached(_layout.AreaHeight))
                    _heightLimitReached.OnNext(Unit.Default);

                return;
            }

            _dragSession.TryTake(out _);
            PlayExplosion(view);
            _rejected.OnNext(result.FailReason);
        }

        private void DropTowerCube(PointerEventData eventData)
        {
            CubeView view = _draggedView;
            string cubeId = _draggedCubeId;
            ResetGesture();

            if (_hole.ContainsScreenPoint(eventData.position, eventData.pressEventCamera) && _tower.TryRemove(cubeId))
            {
                Vector2 holeCenter = ScreenSpace.WorldToRect(_dragLayer.Layer, _hole.Hole.TransformPoint(_hole.Hole.rect.center));
                PlayAndRelease(view, token => view.FallIntoHoleAsync(holeCenter, token));
                return;
            }

            _towerPresenter.EndDrag(cubeId);
            _returned.OnNext(Unit.Default);
        }

        private void Cancel()
        {
            switch (_state)
            {
                case DragState.Scrolling:
                    _paletteView.ForwardEndDrag(_lastEventData);
                    break;

                case DragState.DraggingNew:
                    _factory.Release(_draggedView);
                    break;

                case DragState.DraggingTower:
                    _towerPresenter.EndDrag(_draggedCubeId);
                    break;
            }

            ResetGesture();
        }

        private void BeginGesture(DragState state, PointerEventData eventData)
        {
            _state = state;
            _pointerId = eventData.pointerId;
            _lastEventData = eventData;
        }

        private void ResetGesture()
        {
            _state = DragState.Idle;
            _lastEventData = null;
            _draggedView = null;
            _draggedColorId = null;
            _draggedCubeId = null;
        }

        private bool IsActivePointer(PointerEventData eventData)
        {
            if (_state == DragState.Idle || eventData.pointerId != _pointerId)
                return false;

            _lastEventData = eventData;
            return true;
        }

        private void FollowPointer(PointerEventData eventData)
        {
            if (ScreenSpace.TryScreenToRect(_dragLayer.Layer, eventData.position, eventData.pressEventCamera, out Vector2 position))
                _draggedView.SetAnchoredPosition(position);
        }

        private void PlayExplosion(CubeView view) => 
            PlayAndRelease(view, view.ExplodeAsync);

        private void PlayAndRelease(CubeView view, Func<CancellationToken, UniTask> animation) => 
            PlayAndReleaseAsync(view, animation, _lifetime.Token).Forget();

        private async UniTaskVoid PlayAndReleaseAsync(
            CubeView view,
            Func<CancellationToken, UniTask> animation,
            CancellationToken token)
        {
            try
            {
                await animation(token);
            }
            catch (OperationCanceledException)
            {
                // Scene is unloading or the view was destroyed.
            }

            if (!token.IsCancellationRequested)
                _factory.Release(view);
        }
    }
}
