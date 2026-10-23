using CubeTower.Presentation.Views;

namespace CubeTower.Presentation.Input
{
    /// <summary>
    /// Hands the dragged cube view over from the drag presenter to the tower presenter.
    /// The cube under the finger becomes the tower cube instead of being destroyed and
    /// recreated - otherwise it would blink at the moment of the drop.
    /// </summary>
    public interface IDragSession
    {
        void Offer(CubeView view);
        bool TryTake(out CubeView view);
    }

    public sealed class DragSession : IDragSession
    {
        private CubeView _offered;

        public void Offer(CubeView view) => 
            _offered = view;

        public bool TryTake(out CubeView view)
        {
            view = _offered;
            _offered = null;
            return view != null;
        }
    }
}
