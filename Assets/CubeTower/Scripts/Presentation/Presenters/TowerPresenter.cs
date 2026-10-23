using System;
using System.Collections.Generic;
using System.Threading;
using CubeTower.Application.Config;
using CubeTower.Core.Common;
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
    /// Shows the tower model. Subscribes to model streams and moves views to where the model says.
    /// <para>
    /// Every animation is "go to the target": when the model changes again while cubes are
    /// still moving, the running animation is cancelled and a new one starts from the current
    /// position. That is why the player may grab a cube "at any moment", even mid-flight.
    /// </para>
    /// </summary>
    public sealed class TowerPresenter : IDisposable
    {
        private readonly IReadOnlyTower _tower;
        private readonly TowerZoneView _zone;
        private readonly TowerLayout _layout;
        private readonly ICubeViewFactory _factory;
        private readonly IGameConfigStore _config;
        private readonly IDragSession _dragSession;
        private readonly ILog _log;
        private readonly Dictionary<string, CubeEntry> _entries = new Dictionary<string, CubeEntry>();
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private bool _isDisposed;

        public TowerPresenter(
            IReadOnlyTower tower,
            TowerZoneView zone,
            TowerLayout layout,
            ICubeViewFactory factory,
            IGameConfigStore config,
            IDragSession dragSession,
            ILog log)
        {
            _tower = tower;
            _zone = zone;
            _layout = layout;
            _factory = factory;
            _config = config;
            _dragSession = dragSession;
            _log = log;
        }

        /// <summary>(cube id, view, phase, pointer data) of a drag that started on a tower cube.</summary>
        public event Action<string, CubeView, CubeDragPhase, PointerEventData> CubeDragEvent;

        public void Initialize()
        {
            _tower.Added.Subscribe(OnCubeAdded).AddTo(_subscriptions);
            _tower.Removed.Subscribe(_ => RelayoutAnimated()).AddTo(_subscriptions);
            _tower.Reset.Subscribe(_ => Rebuild()).AddTo(_subscriptions);
            _zone.DimensionsChanged += OnZoneDimensionsChanged;

            Rebuild();
        }

        /// <summary>
        /// The player grabbed a tower cube: stop its animation and hand the view over.
        /// The cube stays in the model until it is dropped into the hole.
        /// </summary>
        public CubeView BeginDrag(string cubeId)
        {
            if (!_entries.TryGetValue(cubeId, out CubeEntry entry) || entry.IsDragged)
                return null;

            entry.CancelAnimation();
            entry.IsDragged = true;
            return entry.View;
        }

        /// <summary>The cube was not dropped into the hole: it flies back to its place in the tower.</summary>
        public void EndDrag(string cubeId)
        {
            if (!_entries.TryGetValue(cubeId, out CubeEntry entry) || !entry.IsDragged)
                return;

            entry.IsDragged = false;
            entry.View.SetRaycastTarget(true);
            ScreenSpace.Reparent(entry.View.Rect, _zone.CubesRoot);
            RelayoutAnimated();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _zone.DimensionsChanged -= OnZoneDimensionsChanged;
            _subscriptions.Dispose();
            _lifetime.Cancel();

            foreach (CubeEntry entry in _entries.Values)
                entry.Dispose();

            _entries.Clear();
            _lifetime.Dispose();
        }

        private void OnCubeAdded(CubeAdded added)
        {
            CubeView view = _dragSession.TryTake(out CubeView dragged) ? dragged : _factory.Create(_zone.CubesRoot);
            ScreenSpace.Reparent(view.Rect, _zone.CubesRoot);
            view.SetRaycastTarget(true);

            CubeEntry entry = CreateEntry(added.Cube, view);
            Vector2 target = _layout.GetCubePosition(_tower, added.Index);
            float cubeSize = _layout.CubeSize;
            RunAnimation(entry, token => entry.View.JumpToAsync(target, cubeSize, token));
        }

        private void Rebuild()
        {
            Dictionary<string, CubeEntry> keptDragged = new();
            
            foreach (CubeEntry entry in _entries.Values)
            {
                if (entry.IsDragged && _tower.TryGetIndex(entry.CubeId, out _))
                {
                    keptDragged.Add(entry.CubeId, entry);
                    continue;
                }

                entry.Dispose();
                entry.View.DragEvent -= OnViewDragEvent;
                
                if (!entry.IsDragged)
                    _factory.Release(entry.View);
            }

            _entries.Clear();

            foreach (KeyValuePair<string, CubeEntry> pair in keptDragged)
                _entries.Add(pair.Key, pair.Value);

            for (int i = 0; i < _tower.Count; i++)
            {
                if (_entries.ContainsKey(_tower.Cubes[i].Id))
                    continue;

                CubeView view = _factory.Create(_zone.CubesRoot);
                CubeEntry entry = CreateEntry(_tower.Cubes[i], view);
                view.SetAnchoredPosition(_layout.GetCubePosition(_tower, i));
                RunAnimation(entry, token => entry.View.PopInAsync(token));
            }
        }

        private void RelayoutAnimated()
        {
            HashSet<string> alive = new();
            
            foreach (TowerCube cube in _tower.Cubes)
                alive.Add(cube.Id);

            List<string> removed = new();
            
            foreach (KeyValuePair<string, CubeEntry> pair in _entries)
                if (!alive.Contains(pair.Key))
                    removed.Add(pair.Key);

            foreach (string id in removed)
            {
                CubeEntry entry = _entries[id];
                entry.Dispose();
                entry.View.DragEvent -= OnViewDragEvent;
                _entries.Remove(id);

                if (!entry.IsDragged)
                    _factory.Release(entry.View);
            }

            for (int i = 0; i < _tower.Count; i++)
            {
                if (!_entries.TryGetValue(_tower.Cubes[i].Id, out CubeEntry entry) || entry.IsDragged)
                    continue;

                Vector2 target = _layout.GetCubePosition(_tower, i);

                if ((entry.View.Rect.anchoredPosition - target).sqrMagnitude > 0.01f)
                    RunAnimation(entry, token => entry.View.MoveToAsync(target, token));
            }
        }

        private void OnZoneDimensionsChanged()
        {
            for (int i = 0; i < _tower.Count; i++)
            {
                if (_entries.TryGetValue(_tower.Cubes[i].Id, out CubeEntry entry) && !entry.IsDragged)
                {
                    entry.CancelAnimation();
                    entry.View.SetSize(_layout.CubeSize);
                    entry.View.SetAnchoredPosition(_layout.GetCubePosition(_tower, i));
                }
            }
        }

        private CubeEntry CreateEntry(TowerCube cube, CubeView view)
        {
            view.SetSize(_layout.CubeSize);

            if (_config.TryGetDefinition(cube.ColorId, out CubeColorDefinition definition))
                view.SetAppearance(definition.Sprite, definition.Color);
            else
                _log.Warning($"Unknown color '{cube.ColorId}' of cube '{cube.Id}'.");

            view.DragEvent += OnViewDragEvent;
            CubeEntry entry = new CubeEntry(cube.Id, view);
            _entries[cube.Id] = entry;
            return entry;
        }

        private void OnViewDragEvent(CubeView view, CubeDragPhase phase, PointerEventData eventData)
        {
            foreach (CubeEntry entry in _entries.Values)
            {
                if (entry.View == view)
                {
                    CubeDragEvent?.Invoke(entry.CubeId, view, phase, eventData);
                    return;
                }
            }
        }

        private void RunAnimation(CubeEntry entry, Func<CancellationToken, UniTask> animation)
        {
            CancellationToken token = entry.RestartAnimation(_lifetime.Token);
            RunAnimationAsync(animation, token).Forget();
        }

        private async UniTaskVoid RunAnimationAsync(Func<CancellationToken, UniTask> animation, CancellationToken token)
        {
            try
            {
                await animation(token);
            }
            catch (OperationCanceledException)
            {
                // Expected: the cube got a new target or was grabbed.
            }
        }

        /// <summary>
        /// A view of a tower cube plus the cancellation source of its current animation.
        /// Owns the CancellationTokenSource, so it must be disposed (a linked source keeps
        /// a registration in the parent token until disposed).
        /// </summary>
        private sealed class CubeEntry : IDisposable
        {
            private CancellationTokenSource _animation;

            public CubeEntry(string cubeId, CubeView view)
            {
                CubeId = cubeId;
                View = view;
            }

            public string CubeId { get; }
            public CubeView View { get; }
            public bool IsDragged { get; set; }

            public CancellationToken RestartAnimation(CancellationToken lifetime)
            {
                CancelAnimation();
                _animation = CancellationTokenSource.CreateLinkedTokenSource(lifetime);
                return _animation.Token;
            }

            public void CancelAnimation()
            {
                if (_animation == null)
                    return;

                _animation.Cancel();
                _animation.Dispose();
                _animation = null;
            }

            public void Dispose() => 
                CancelAnimation();
        }
    }
}
