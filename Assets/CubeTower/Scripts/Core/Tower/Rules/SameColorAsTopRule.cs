namespace CubeTower.Core.Tower.Rules
{
    /// <summary>
    /// Example of extending the game from the technical task:
    /// "a new cube can be placed only on a cube of the same color".
    /// Not registered by default. To enable it, add one binding in the installer
    /// </summary>
    public sealed class SameColorAsTopRule : IPlacementRule
    {
        public PlacementResult Check(IReadOnlyTower tower, PlacementRequest request)
        {
            if (tower.Count == 0)
                return PlacementResult.Allowed();

            TowerCube top = tower.Cubes[tower.Count - 1];
            return top.ColorId == request.ColorId
                ? PlacementResult.Allowed()
                : PlacementResult.Fail(PlacementFailReasons.ColorMismatch);
        }
    }
}
