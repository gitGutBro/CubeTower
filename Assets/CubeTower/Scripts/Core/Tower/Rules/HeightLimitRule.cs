namespace CubeTower.Core.Tower.Rules
{
    /// <summary>
    /// The height limit is the top of the screen. If the last cube went beyond it,
    /// the tower cannot be built higher. The cube that crosses the border is still placed.
    /// </summary>
    public sealed class HeightLimitRule : IPlacementRule
    {
        public PlacementResult Check(IReadOnlyTower tower, PlacementRequest request)
        {
            return tower.IsHeightLimitReached(request.AreaHeight)
                ? PlacementResult.Fail(PlacementFailReasons.HeightLimit)
                : PlacementResult.Allowed();
        }
    }
}
