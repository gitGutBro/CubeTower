namespace CubeTower.Presentation.Views
{
    public enum CubeDragPhase
    {
        /// <summary>Pointer pressed; a drag may follow. Used to stop scroll inertia.</summary>
        InitializePotentialDrag,
        Begin,
        Drag,
        End,
    }
}
