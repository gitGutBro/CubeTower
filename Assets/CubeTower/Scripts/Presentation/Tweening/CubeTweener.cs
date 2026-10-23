using System;
using CubeTower.Presentation.Settings;
using DG.Tweening;
using UnityEngine;

namespace CubeTower.Presentation.Tweening
{
    /// <summary>
    /// Builds cube animations. Views stay passive ("how it looks" lives here),
    /// presenters stay free of DOTween ("what and when" lives there).
    /// Only DOTween core API is used (no DOTween Modules), so no extra setup is needed.
    /// Every tween is linked to its GameObject (killed when it is destroyed) and
    /// targeted to its RectTransform (so all tweens of a cube can be killed at once).
    /// </summary>
    public sealed class CubeTweener
    {
        private readonly AnimationSettings _settings;

        public CubeTweener(AnimationSettings settings) => 
            _settings = settings ? settings : throw new ArgumentNullException(nameof(settings));

        /// <summary>Jump along a parabola to the target (placing a cube on the tower).</summary>
        public Tween Jump(RectTransform rect, Vector2 target, float cubeSize)
        {
            Vector2 start = rect.anchoredPosition;
            float height = _settings.JumpHeight * cubeSize;
            float progress = 0f;

            return Link(
                DOTween.To(
                    () => progress,
                    value =>
                    {
                        progress = value;
                        Vector2 position = Vector2.LerpUnclamped(start, target, value);
                        position.y += 4f * height * value * (1f - value);
                        rect.anchoredPosition = position;
                    },
                    1f,
                    _settings.JumpDuration).SetEase(Ease.Linear),
                rect);
        }

        /// <summary>Smooth move (cubes above a removed one fall down, a cube returns to its slot).</summary>
        public Tween MoveTo(RectTransform rect, Vector2 target)
        {
            return Link(
                DOTween.To(() => rect.anchoredPosition, value => rect.anchoredPosition = value, target, _settings.MoveDuration)
                    .SetEase(_settings.MoveEase),
                rect);
        }

        /// <summary>Appear from zero scale (tower restored from a save).</summary>
        public Tween PopIn(RectTransform rect)
        {
            rect.localScale = Vector3.zero;
            return Link(rect.DOScale(1f, _settings.PopInDuration).SetEase(Ease.OutBack), rect);
        }

        /// <summary>"Explosion" without particles: punch, shake, then shrink, spin and fade out.</summary>
        public Tween Explode(RectTransform rect, CanvasGroup canvasGroup)
        {
            float duration = _settings.ExplodeDuration;
            float punchPart = duration * 0.35f;
            float vanishPart = duration - punchPart;

            Sequence sequence = DOTween.Sequence()
                .Append(rect.DOPunchScale(Vector3.one * 0.35f, punchPart, 10, 0.6f))
                .Join(rect.DOShakePosition(punchPart, 12f, 30, 90f, false, true))
                .Append(rect.DOScale(0f, vanishPart).SetEase(Ease.InBack))
                .Join(rect.DOLocalRotate(new Vector3(0f, 0f, 120f), vanishPart, RotateMode.FastBeyond360))
                .Join(Fade(canvasGroup, 0f, vanishPart));

            return Link(sequence, rect);
        }

        /// <summary>Fly into the hole center, then shrink and spin as if falling down.</summary>
        public Tween FallIntoHole(RectTransform rect, CanvasGroup canvasGroup, Vector2 holeCenter)
        {
            float duration = _settings.FallIntoHoleDuration;
            float flyPart = duration * 0.45f;
            float fallPart = duration - flyPart;

            Sequence sequence = DOTween.Sequence()
                .Append(DOTween.To(() => rect.anchoredPosition, value => rect.anchoredPosition = value, holeCenter, flyPart)
                    .SetEase(Ease.OutQuad))
                .Append(rect.DOScale(0f, fallPart).SetEase(Ease.InQuad))
                .Join(rect.DOLocalRotate(new Vector3(0f, 0f, 270f), fallPart, RotateMode.FastBeyond360))
                .Join(Fade(canvasGroup, 0f, fallPart));

            return Link(sequence, rect);
        }

        private static Tween Fade(CanvasGroup canvasGroup, float target, float duration) => 
            DOTween.To(
                () => canvasGroup.alpha, 
                value => canvasGroup.alpha = value, 
                target, duration);

        private static Tween Link(Tween tween, RectTransform rect) => 
            tween.SetTarget(rect).SetLink(rect.gameObject);
    }
}
