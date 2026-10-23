using System;
using CubeTower.Presentation.Settings;
using DG.Tweening;
using UnityEngine;

namespace CubeTower.Presentation.Tweening
{
    /// <summary>
    /// Fade animations for UI groups: the message above the bottom scroll and the loading curtain.
    /// Uses DOTween.To instead of CanvasGroup.DOFade, which lives in DOTween Modules.
    /// </summary>
    public sealed class FadeTweener
    {
        private readonly AnimationSettings _settings;

        public FadeTweener(AnimationSettings settings) => 
            _settings = settings ? settings : throw new ArgumentNullException(nameof(settings));

        /// <summary>Fade in, hold, fade out.</summary>
        public Tween Flash(CanvasGroup group)
        {
            Sequence sequence = DOTween.Sequence()
                .Append(CreateFade(group, 1f, _settings.MessageFadeInDuration))
                .AppendInterval(_settings.MessageHoldDuration)
                .Append(CreateFade(group, 0f, _settings.MessageFadeOutDuration));

            return sequence.SetTarget(group).SetLink(group.gameObject);
        }

        public static Tween Fade(CanvasGroup group, float target, float duration) => 
            CreateFade(group, target, duration).SetTarget(group).SetLink(group.gameObject);

        private static Tween CreateFade(CanvasGroup group, float target, float duration) => 
            DOTween.To(() => group.alpha, value => group.alpha = value, target, duration);
    }
}
