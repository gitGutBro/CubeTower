using CubeTower.Core.Geometry;
using UnityEngine;

namespace CubeTower.Presentation.Views
{
    /// <summary>
    /// The oval hole in the left top part. The hit test respects the oval shape:
    /// a drop into the rectangle corners outside the ellipse is a miss.
    /// </summary>
    public sealed class HoleView : MonoBehaviour
    {
        [SerializeField] private RectTransform _hole;

        public RectTransform Hole => _hole;

        public bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_hole, screenPoint, eventCamera, out Vector2 local))
                return false;

            Rect rect = _hole.rect;
            Vector2 fromCenter = local - rect.center;
            return Ellipse.Contains(fromCenter.x, fromCenter.y, rect.width * 0.5f, rect.height * 0.5f);
        }
    }
}
