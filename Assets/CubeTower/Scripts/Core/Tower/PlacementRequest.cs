namespace CubeTower.Core.Tower
{
    /// <summary>
    /// Everything the tower needs to decide whether a dropped cube can be placed.
    /// All values are in cube units inside the tower area (origin = bottom-left corner).
    /// </summary>
    public readonly struct PlacementRequest
    {
        public PlacementRequest(string colorId, float dropX, float dropY, float areaWidth, float areaHeight)
        {
            ColorId = colorId;
            DropX = dropX;
            DropY = dropY;
            AreaWidth = areaWidth;
            AreaHeight = areaHeight;
        }

        public string ColorId { get; }
        public float DropX { get; }
        public float DropY { get; }
        public float AreaWidth { get; }

        /// <summary>Height of the tower area. Its top edge is the top of the screen.</summary>
        public float AreaHeight { get; }
    }
}
