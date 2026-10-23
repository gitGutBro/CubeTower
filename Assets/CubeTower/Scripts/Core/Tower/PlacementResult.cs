namespace CubeTower.Core.Tower
{
    public readonly struct PlacementResult
    {
        private PlacementResult(bool isSuccess, string failReason, TowerCube cube)
        {
            IsSuccess = isSuccess;
            FailReason = failReason;
            Cube = cube;
        }

        public bool IsSuccess { get; }

        /// <summary>One of <see cref="PlacementFailReasons"/> (or a custom reason) when the placement failed.</summary>
        public string FailReason { get; }

        /// <summary>The placed cube when the placement succeeded.</summary>
        public TowerCube Cube { get; }

        public static PlacementResult Allowed() => 
            new(isSuccess: true, failReason: null, cube: null);

        public static PlacementResult Placed(TowerCube cube) => 
            new(isSuccess: true, failReason: null, cube: cube);

        public static PlacementResult Fail(string reason) => 
            new(isSuccess: false, failReason: reason, cube: null);
    }
}
