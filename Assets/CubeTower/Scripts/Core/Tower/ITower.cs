using System.Collections.Generic;
using R3;

namespace CubeTower.Core.Tower
{
    /// <summary>
    /// Read-only view of the tower. Given to subscribers (presenters, messages, autosave)
    /// that must observe the tower but are not allowed to change it.
    /// </summary>
    public interface IReadOnlyTower
    {
        int Count { get; }

        /// <summary>Vertical position of the bottom cube's lower edge, in cube units.</summary>
        float BaseY { get; }

        /// <summary>Upper edge of the top cube, in cube units.</summary>
        float TopEdge { get; }

        IReadOnlyList<TowerCube> Cubes { get; }

        /// <summary>Raised when a cube is placed on top.</summary>
        Observable<CubeAdded> Added { get; }

        /// <summary>Raised when a cube is removed; cubes above have already moved down.</summary>
        Observable<CubeRemoved> Removed { get; }

        /// <summary>Raised when the whole tower was replaced (restore from save, clear, fit to area).</summary>
        Observable<Unit> Reset { get; }

        bool TryGetIndex(string cubeId, out int index);

        /// <summary>
        /// True when the last cube went beyond the top of the area:
        /// the tower cannot be built higher any more.
        /// </summary>
        bool IsHeightLimitReached(float areaHeight);
    }

    /// <summary>
    /// Full access to the tower. Given only to code that changes it.
    /// </summary>
    public interface ITower : IReadOnlyTower
    {
        PlacementResult TryPlace(PlacementRequest request);

        bool TryRemove(string cubeId);

        /// <summary>Moves the tower horizontally/vertically so it fits into an area of a new size.</summary>
        void FitToArea(float areaWidth, float areaHeight);

        void Clear();

        TowerData CreateData();

        /// <summary>Replaces the tower with saved data. Returns false if the data is invalid.</summary>
        bool Restore(TowerData data);
    }
}
