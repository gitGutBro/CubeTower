using UnityEngine;

namespace CubeTower.Presentation.Views
{
    /// <summary>
    /// Full-screen layer above everything. A dragged cube lives here so it is drawn on top
    /// and is not clipped by the scroll mask.
    /// </summary>
    public sealed class DragLayerView : MonoBehaviour
    {
        [SerializeField] private RectTransform _layer;
        [SerializeField] private RectTransform _poolRoot;

        public RectTransform Layer => _layer;

        /// <summary>Inactive container for pooled cube views.</summary>
        public RectTransform PoolRoot => _poolRoot;
    }
}
