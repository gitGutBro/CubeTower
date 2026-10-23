namespace CubeTower.Core.Tower.Rules
{
    /// <summary>
    /// One condition that must hold for a cube to be placed.
    /// Rules form a chain inside <see cref="Tower"/>: the first failing rule rejects the placement.
    /// New gameplay conditions (for example, "only on a cube of the same color")
    /// are added as new rule classes without changing existing code (Open/Closed principle).
    /// </summary>
    public interface IPlacementRule
    {
        PlacementResult Check(IReadOnlyTower tower, PlacementRequest request);
    }
}
