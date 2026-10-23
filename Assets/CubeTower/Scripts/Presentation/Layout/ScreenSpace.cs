using UnityEngine;

namespace CubeTower.Presentation.Layout
{
    /// <summary>
    /// Coordinate helpers. All cube views are anchored to the bottom-left corner of their parent,
    /// so "anchored position" = position from the parent's bottom-left corner.
    /// </summary>
    public static class ScreenSpace
    {
        /// <summary>Screen point -> position from the bottom-left corner of the rect.</summary>
        public static bool TryScreenToRect(RectTransform rect, Vector2 screenPoint, Camera eventCamera, out Vector2 position)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, eventCamera, out Vector2 local))
            {
                position = local - rect.rect.min;
                return true;
            }

            position = default;
            return false;
        }

        /// <summary>World point -> position from the bottom-left corner of the rect.</summary>
        public static Vector2 WorldToRect(RectTransform rect, Vector3 worldPoint) => 
            (Vector2)rect.InverseTransformPoint(worldPoint) - rect.rect.min;

        /// <summary>Moves a view to another parent keeping it visually in place.</summary>
        public static void Reparent(RectTransform view, RectTransform newParent)
        {
            view.SetParent(newParent, true);
            view.anchoredPosition = WorldToRect(newParent, view.position);
        }
    }
}
