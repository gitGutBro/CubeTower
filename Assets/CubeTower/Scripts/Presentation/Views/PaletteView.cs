using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeTower.Presentation.Views
{
    /// <summary>
    /// Bottom horizontal scroll. The cubes in it receive the pointer events themselves,
    /// so the ScrollRect would never see a drag. When a presenter decides that a gesture
    /// is a scroll, it forwards the events here and the view passes them to the ScrollRect.
    /// </summary>
    public sealed class PaletteView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _content;
        [SerializeField] private HorizontalLayoutGroup _layout;

        public event Action DimensionsChanged;

        public RectTransform Content => _content;
        public float ViewportHeight => _viewport.rect.height;

        public void SetLayout(float spacing, float padding)
        {
            int paddingPixels = Mathf.RoundToInt(padding);
            _layout.spacing = spacing;
            _layout.padding = new RectOffset(paddingPixels, paddingPixels, 0, 0);
            LayoutRebuilder.MarkLayoutForRebuild(_content);
        }

        public void ForwardInitializePotentialDrag(PointerEventData eventData) => 
            _scrollRect.OnInitializePotentialDrag(eventData);

        public void ForwardBeginDrag(PointerEventData eventData) => 
            _scrollRect.OnBeginDrag(eventData);

        public void ForwardDrag(PointerEventData eventData) => 
            _scrollRect.OnDrag(eventData);

        public void ForwardEndDrag(PointerEventData eventData) => 
            _scrollRect.OnEndDrag(eventData);

        private void OnRectTransformDimensionsChange() => 
            DimensionsChanged?.Invoke();
    }
}
