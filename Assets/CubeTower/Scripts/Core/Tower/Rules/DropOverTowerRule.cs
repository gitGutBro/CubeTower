namespace CubeTower.Core.Tower.Rules
{
    /// <summary>
    /// The first cube can be placed anywhere in the area.
    /// Every next cube must be dropped over the cubes already placed
    /// (or right above the top cube); then it jumps to the top of the tower.
    /// </summary>
    public sealed class DropOverTowerRule : IPlacementRule
    {
        public PlacementResult Check(IReadOnlyTower tower, PlacementRequest request)
        {
            if (tower.Count == 0)
                return PlacementResult.Allowed();

            for (int i = 0; i < tower.Count; i++)
                if (IsInsideCube(tower.Cubes[i].X, tower.BaseY + i, request))
                    return PlacementResult.Allowed();

            TowerCube top = tower.Cubes[tower.Count - 1];
            return IsInsideCube(top.X, tower.TopEdge, request)
                ? PlacementResult.Allowed()
                : PlacementResult.Fail(PlacementFailReasons.MissedTower);
        }

        private static bool IsInsideCube(float centerX, float bottomY, PlacementRequest request)
        {
            return request.DropX >= centerX - TowerGeometry.HalfCube
                && request.DropX <= centerX + TowerGeometry.HalfCube
                && request.DropY >= bottomY
                && request.DropY <= bottomY + 1f;
        }
    }
}
