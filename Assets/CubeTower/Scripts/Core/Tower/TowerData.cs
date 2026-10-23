using System.Collections.Generic;

namespace CubeTower.Core.Tower
{
    /// <summary>
    /// Save-file DTO of the tower. Contains only what is needed to rebuild the tower:
    /// derived state (height limit flag, layout) is recalculated after loading.
    /// The single public constructor lets Newtonsoft.Json build an immutable object.
    /// </summary>
    public sealed class TowerData
    {
        public const int CurrentVersion = 1;

        public TowerData(int version, float baseY, IReadOnlyList<TowerCubeData> cubes)
        {
            Version = version;
            BaseY = baseY;
            Cubes = cubes;
        }

        public int Version { get; }
        public float BaseY { get; }
        public IReadOnlyList<TowerCubeData> Cubes { get; }
    }

    public sealed class TowerCubeData
    {
        public TowerCubeData(string id, string colorId, float x)
        {
            Id = id;
            ColorId = colorId;
            X = x;
        }

        public string Id { get; }
        public string ColorId { get; }
        public float X { get; }
    }
}
