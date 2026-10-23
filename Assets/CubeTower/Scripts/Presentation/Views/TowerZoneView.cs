using System;
using UnityEngine;

namespace CubeTower.Presentation.Views
{
    /// <summary>
    /// Right top part of the screen where the tower is built.
    /// Its top edge is the top of the screen, which is the tower height limit.
    /// </summary>
    public sealed class TowerZoneView : MonoBehaviour
    {
        [SerializeField] private RectTransform _area;
        [SerializeField] private RectTransform _cubesRoot;

        public event Action DimensionsChanged;

        /// <summary>Drop area; also defines the tower coordinate system (origin = bottom-left).</summary>
        public RectTransform Area => _area;

        /// <summary>Parent of tower cubes. Stretched over <see cref="Area"/>.</summary>
        public RectTransform CubesRoot => _cubesRoot;

        private void OnRectTransformDimensionsChange() => 
            DimensionsChanged?.Invoke();
    }
}
