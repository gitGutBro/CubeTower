using CubeTower.Presentation.Settings;
using CubeTower.Presentation.Views;
using UnityEngine;

namespace CubeTower.Presentation.Layout
{
    /// <summary>
    /// Cube size in canvas units. Derived from the bottom scroll height,
    /// so cubes keep proportions on 19.5:9, 16:9 and 4:3 screens.
    /// </summary>
    public sealed class CubeMetrics
    {
        private readonly PaletteView _palette;
        private readonly PresentationSettings _settings;

        public CubeMetrics(PaletteView palette, PresentationSettings settings)
        {
            _palette = palette;
            _settings = settings;
        }

        public float Size => 
            Mathf.Max(1f, _palette.ViewportHeight * _settings.CubeSizeToPaletteHeight);
    }
}
