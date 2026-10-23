namespace CubeTower.Core.Tower
{
    /// <summary>A cube was placed on top of the tower.</summary>
    public readonly struct CubeAdded
    {
        public CubeAdded(int index, TowerCube cube)
        {
            Index = index;
            Cube = cube;
        }

        public int Index { get; }
        public TowerCube Cube { get; }
    }

    /// <summary>A cube was thrown out of the tower. Cubes above it moved down by one.</summary>
    public readonly struct CubeRemoved
    {
        public CubeRemoved(int index, TowerCube cube)
        {
            Index = index;
            Cube = cube;
        }

        public int Index { get; }
        public TowerCube Cube { get; }
    }
}
