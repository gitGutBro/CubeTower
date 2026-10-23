using System;

namespace CubeTower.Core.Tower
{
    /// <summary>
    /// Immutable cube that stands in the tower.
    /// Coordinates are measured in "cube units": one unit equals one cube edge,
    /// so the domain does not depend on pixels or screen resolution.
    /// </summary>
    public sealed class TowerCube
    {
        public TowerCube(string id, string colorId, float x)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Cube id must not be empty.", nameof(id));

            if (string.IsNullOrEmpty(colorId))
                throw new ArgumentException("Color id must not be empty.", nameof(colorId));

            Id = id;
            ColorId = colorId;
            X = x;
        }

        public string Id { get; }
        public string ColorId { get; }

        /// <summary>Horizontal center of the cube inside the tower area, in cube units.</summary>
        public float X { get; }

        public TowerCube WithX(float x) => 
            new(Id, ColorId, x);
    }
}
