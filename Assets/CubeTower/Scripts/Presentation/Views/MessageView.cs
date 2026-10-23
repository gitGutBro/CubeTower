using System;
using CubeTower.Presentation.Tweening;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CubeTower.Presentation.Views
{
    /// <summary>
    /// Text above the bottom part of the screen that comments every action.
    /// A new message replaces the current one.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class MessageView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _canvasGroup;

        private FadeTweener _tweener;

        public void Initialize(FadeTweener tweener)
        {
            _tweener = tweener ?? throw new ArgumentNullException(nameof(tweener));
            _canvasGroup.alpha = 0f;
        }

        public void Show(string text)
        {
            if (_tweener == null)
                throw new InvalidOperationException($"{nameof(MessageView)} is not constructed.");

            DOTween.Kill(_canvasGroup);
            _text.text = text;
            _tweener.Flash(_canvasGroup);
        }
    }
}
