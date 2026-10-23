namespace CubeTower.Core.Tower
{
    /// <summary>
    /// Reasons why a placement was rejected. Strings instead of an enum:
    /// a new rule can introduce a new reason without editing this file.
    /// The presentation layer maps reasons to localized messages.
    /// </summary>
    public static class PlacementFailReasons
    {
        public const string MissedTower = "placement.missed_tower";
        public const string HeightLimit = "placement.height_limit";
        public const string ColorMismatch = "placement.color_mismatch";
        public const string InvalidRequest = "placement.invalid_request";
    }
}
