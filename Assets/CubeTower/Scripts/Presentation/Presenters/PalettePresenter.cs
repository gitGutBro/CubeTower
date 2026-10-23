using System;
using System.Collections.Generic;
using CubeTower.Application.Config;
using CubeTower.Presentation.Factories;
using CubeTower.Presentation.Layout;
using CubeTower.Presentation.Settings;
using CubeTower.Presentation.Views;
using UnityEngine.EventSystems;

namespace CubeTower.Presentation.Presenters
{
    /// <summary>
    /// Fills the bottom scroll with cubes from the config. The palette is infinite:
    /// its cubes never leave it, a drag spawns a copy.
    /// </summary>
    public sealed class PalettePresenter : IDisposable
    {
        private readonly PaletteView _view;
        private readonly ICubeViewFactory _factory;
        private readonly IGameConfigStore _config;
        private readonly CubeMetrics _metrics;
        private readonly PresentationSettings _settings;
        private readonly Dictionary<CubeView, string> _cells = new();

        public PalettePresenter(
            PaletteView view,
            ICubeViewFactory factory,
            IGameConfigStore config,
            CubeMetrics metrics,
            PresentationSettings settings)
        {
            _view = view;
            _factory = factory;
            _config = config;
            _metrics = metrics;
            _settings = settings;
        }

        /// <summary>(color id, phase, pointer data) of a drag that started on a palette cube.</summary>
        public event Action<string, CubeDragPhase, PointerEventData> CellDragEvent;

        public void Initialize()
        {
            GameConfig config = _config.Current;

            for (int i = 0; i < config.PaletteSize; i++)
            {
                CubeColorDefinition color = config.Colors[i];
                CubeView cell = _factory.Create(_view.Content);
                cell.SetAppearance(color.Sprite, color.Color);
                cell.DragEvent += OnCellDragEvent;
                _cells.Add(cell, color.Id);
            }

            ApplySize();
            _view.DimensionsChanged += ApplySize;
        }

        public void Dispose()
        {
            _view.DimensionsChanged -= ApplySize;

            foreach (CubeView cell in _cells.Keys)
                if (cell != null)
                    cell.DragEvent -= OnCellDragEvent;

            _cells.Clear();
        }

        private void ApplySize()
        {
            float size = _metrics.Size;
            
            foreach (CubeView cell in _cells.Keys)
                cell.SetSize(size);

            _view.SetLayout(size * _settings.PaletteSpacing, size * _settings.PaletteSpacing);
        }

        private void OnCellDragEvent(CubeView cell, CubeDragPhase phase, PointerEventData eventData)
        {
            if (_cells.TryGetValue(cell, out string colorId))
                CellDragEvent?.Invoke(colorId, phase, eventData);
        }
    }
}
