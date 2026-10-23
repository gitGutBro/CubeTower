using System;

namespace CubeTower.Core.Tower
{
    /// <summary>
    /// Geometric constants and helpers of the tower in cube units.
    /// </summary>
    public static class TowerGeometry
    {
        /// <summary>A cube may be shifted relative to the cube below by no more than 50% of its edge.</summary>
        public const float MaxHorizontalOffset = 0.5f;
        public const float HalfCube = 0.5f;

        /// <summary>Tolerance for float comparisons of positions restored from a save file.</summary>
        public const float Epsilon = 0.0001f;

        public static float Clamp(float value, float min, float max)
        {
            if (min > max)
                return (min + max) * 0.5f;

            return Math.Max(min, Math.Min(max, value));
        }

        public static bool IsFinite(float value) => 
            !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
