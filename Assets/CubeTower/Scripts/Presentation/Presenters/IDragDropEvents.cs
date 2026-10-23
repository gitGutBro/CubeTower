using R3;

namespace CubeTower.Presentation.Presenters
{
    /// <summary>
    /// Outcomes of drops that are not changes of the tower model.
    /// A narrow interface: the message presenter depends on these events,
    /// not on the drag-and-drop presenter class.
    /// </summary>
    public interface IDragDropEvents
    {
        /// <summary>A cube from the scroll was dropped outside the tower zone.</summary>
        Observable<Unit> Missed { get; }

        /// <summary>The tower rejected a cube; the value is a reason from PlacementFailReasons.</summary>
        Observable<string> Rejected { get; }

        /// <summary>A tower cube was dropped outside the hole and flies back.</summary>
        Observable<Unit> Returned { get; }

        /// <summary>The last placed cube crossed the top of the screen.</summary>
        Observable<Unit> HeightLimitReached { get; }
    }
}
