using System.Collections.Generic;

namespace CubeTower.Application.Localization
{
    /// <summary>
    /// All text keys of the game in one place: no magic strings in the code,
    /// and the editor validator can check that every key has translations.
    /// </summary>
    public static class LocalizationKeys
    {
        public const string CubePlaced = "message.cube_placed";
        public const string CubeRemoved = "message.cube_removed";
        public const string CubeMissed = "message.cube_missed";
        public const string CubeReturned = "message.cube_returned";
        public const string HeightLimit = "message.height_limit";
        public const string TowerFull = "message.tower_full";
        public const string ColorMismatch = "message.color_mismatch";
        public const string PlacementRejected = "message.placement_rejected";
        public const string Loading = "curtain.loading";
        public const string StartupError = "curtain.startup_error";

        public static IReadOnlyList<string> All { get; } = new[]
        {
            CubePlaced,
            CubeRemoved,
            CubeMissed,
            CubeReturned,
            HeightLimit,
            TowerFull,
            ColorMismatch,
            PlacementRejected,
            Loading,
            StartupError,
        };
    }
}
