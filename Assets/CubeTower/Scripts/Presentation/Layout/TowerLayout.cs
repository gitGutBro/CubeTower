using CubeTower.Core.Tower;
using CubeTower.Presentation.Views;
using UnityEngine;

namespace CubeTower.Presentation.Layout
{
    /// <summary>
    /// Converts between the tower model (cube units) and the tower zone (canvas units).
    /// The model never sees pixels; this class is the only place that knows both worlds.
    /// </summary>
    public sealed class TowerLayout
    {
        private readonly TowerZoneView _zone;
        private readonly CubeMetrics _metrics;

        public TowerLayout(TowerZoneView zone, CubeMetrics metrics)
        {
            _zone = zone;
            _metrics = metrics;
        }

        public float CubeSize => _metrics.Size;
        public float AreaWidth => _zone.Area.rect.width / CubeSize;
        public float AreaHeight => _zone.Area.rect.height / CubeSize;

        /// <summary>Center of the cube with the given index, relative to the bottom-left of the cubes root.</summary>
        public Vector2 GetCubePosition(IReadOnlyTower tower, int index)
        {
            TowerCube cube = tower.Cubes[index];
            float size = CubeSize;
            return new Vector2(cube.X * size, (tower.BaseY + index + 0.5f) * size);
        }

        /// <summary>Screen point -> tower coordinates. False if the point is outside the tower zone.</summary>
        public bool TryGetTowerPoint(Vector2 screenPoint, Camera eventCamera, out Vector2 towerPoint)
        {
            towerPoint = default;

            if (!RectTransformUtility.RectangleContainsScreenPoint(_zone.Area, screenPoint, eventCamera)
                || !ScreenSpace.TryScreenToRect(_zone.Area, screenPoint, eventCamera, out Vector2 position))
            {
                return false;
            }

            towerPoint = position / CubeSize;
            return true;
        }
    }
}
