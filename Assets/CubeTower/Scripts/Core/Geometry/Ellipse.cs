namespace CubeTower.Core.Geometry
{
    /// <summary>
    /// Ellipse math used for the oval hole hit test.
    /// </summary>
    public static class Ellipse
    {
        /// <summary>
        /// Checks whether a point lies inside an axis-aligned ellipse.
        /// The point is given relative to the ellipse center.
        /// </summary>
        public static bool Contains(float x, float y, float radiusX, float radiusY)
        {
            if (radiusX <= 0f || radiusY <= 0f)
                return false;

            float nx = x / radiusX;
            float ny = y / radiusY;
            return (nx * nx) + (ny * ny) <= 1f;
        }
    }
}
