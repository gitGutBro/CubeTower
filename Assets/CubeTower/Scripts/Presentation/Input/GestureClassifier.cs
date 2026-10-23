using UnityEngine;

namespace CubeTower.Presentation.Input
{
    public enum DragIntent
    {
        Undetermined,
        Scroll,
        PickUp,
    }

    /// <summary>
    /// Decides what a drag started in the bottom scroll means: mostly horizontal -> scroll,
    /// otherwise -> pick up the cube. The decision is made once, when the drag starts,
    /// so a started drag never turns into a scroll and vice versa.
    /// </summary>
    public sealed class GestureClassifier
    {
        private readonly float _scrollMaxAngle;

        public GestureClassifier(float scrollMaxAngle) => 
            _scrollMaxAngle = Mathf.Clamp(scrollMaxAngle, 0f, 90f);

        public DragIntent Classify(Vector2 delta)
        {
            if (delta.sqrMagnitude < Mathf.Epsilon)
                return DragIntent.Undetermined;

            float angle = Mathf.Atan2(Mathf.Abs(delta.y), Mathf.Abs(delta.x)) * Mathf.Rad2Deg;
            return angle < _scrollMaxAngle ? DragIntent.Scroll : DragIntent.PickUp;
        }
    }
}
