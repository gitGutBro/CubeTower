using System;
using System.Threading;
using CubeTower.Presentation.Tweening;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeTower.Presentation.Views
{
    /// <summary>
    /// Passive view of one cube. The same prefab is used in the bottom scroll, in the tower
    /// and while dragging. It does not decide anything: it reports pointer events
    /// and executes commands of presenters. Animation details are delegated to <see cref="CubeTweener"/>.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public sealed class CubeView : MonoBehaviour,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private Image _image;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LayoutElement _layoutElement;

        private CubeTweener _tweener;
        private Sprite _defaultSprite;

        /// <summary>Pointer events, forwarded as-is. Presenters decide what they mean.</summary>
        public event Action<CubeView, CubeDragPhase, PointerEventData> DragEvent;

        public RectTransform Rect => _rect;

        public void Initialize(CubeTweener tweener, Sprite defaultSprite)
        {
            _tweener = tweener ?? throw new ArgumentNullException(nameof(tweener));
            _defaultSprite = defaultSprite != null ? defaultSprite : _image.sprite;
        }

        /// <summary>
        /// A ready-made sprite is shown as is; without it the default sprite is tinted with the color.
        /// </summary>
        public void SetAppearance(Sprite sprite, Color color)
        {
            bool hasOwnSprite = sprite != null;
            _image.sprite = hasOwnSprite ? sprite : _defaultSprite;
            _image.color = hasOwnSprite ? Color.white : color;
            _image.type = _image.sprite != null && _image.sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
        }

        public void SetSize(float size)
        {
            _rect.sizeDelta = new Vector2(size, size);

            if (_layoutElement != null)
            {
                _layoutElement.preferredWidth = size;
                _layoutElement.preferredHeight = size;
            }
        }

        /// <summary>A dragged cube must not block raycasts, otherwise it hides what is under the finger.</summary>
        public void SetRaycastTarget(bool isTarget) => 
            _canvasGroup.blocksRaycasts = isTarget;

        public void SetAnchoredPosition(Vector2 position) => 
            _rect.anchoredPosition = position;

        /// <summary>Returns the view to a clean state before reuse from the pool.</summary>
        public void ResetState()
        {
            StopAnimations();
            DragEvent = null;

            _rect.anchorMin = Vector2.zero;
            _rect.anchorMax = Vector2.zero;
            _rect.pivot = new Vector2(0.5f, 0.5f);
            _rect.localScale = Vector3.one;
            _rect.localRotation = Quaternion.identity;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        public void StopAnimations() => 
            DOTween.Kill(_rect);

        public UniTask JumpToAsync(Vector2 target, float cubeSize, CancellationToken cancellationToken) =>
            Animate(_tweener.Jump(_rect, target, cubeSize), cancellationToken);

        public UniTask MoveToAsync(Vector2 target, CancellationToken cancellationToken) =>
            Animate(_tweener.MoveTo(_rect, target), cancellationToken);

        public UniTask PopInAsync(CancellationToken cancellationToken) =>
            Animate(_tweener.PopIn(_rect), cancellationToken);

        public UniTask ExplodeAsync(CancellationToken cancellationToken) =>
            Animate(_tweener.Explode(_rect, _canvasGroup), cancellationToken);

        public UniTask FallIntoHoleAsync(Vector2 holeCenter, CancellationToken cancellationToken) =>
            Animate(_tweener.FallIntoHole(_rect, _canvasGroup, holeCenter), cancellationToken);

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData) =>
            DragEvent?.Invoke(this, CubeDragPhase.InitializePotentialDrag, eventData);

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) =>
            DragEvent?.Invoke(this, CubeDragPhase.Begin, eventData);

        void IDragHandler.OnDrag(PointerEventData eventData) =>
            DragEvent?.Invoke(this, CubeDragPhase.Drag, eventData);

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) =>
            DragEvent?.Invoke(this, CubeDragPhase.End, eventData);

        private UniTask Animate(Tween tween, CancellationToken cancellationToken)
        {
            if (_tweener == null)
                throw new InvalidOperationException($"{nameof(CubeView)} is not constructed. Create it through the factory.");

            return tween.AwaitCompletionAsync(cancellationToken);
        }
    }
}
