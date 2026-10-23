using CubeTower.Application.Services;
using CubeTower.Presentation.Tweening;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CubeTower.Presentation.Views
{
    /// <summary>
    /// Lives in the ProjectContext,
    /// so it survives scene loading and covers the switch from bootstrap to the game.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class LoadingCurtainView : MonoBehaviour, ILoadingCurtain
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _status;
        [SerializeField, Min(0.01f)] private float _fadeDuration = 0.3f;

        public void Show(string message)
        {
            DOTween.Kill(_canvasGroup);
            gameObject.SetActive(true);
            _status.text = message;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            DOTween.Kill(_canvasGroup);
            _canvasGroup.blocksRaycasts = false;
            FadeTweener.Fade(_canvasGroup, 0f, _fadeDuration)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void ShowError(string message) => 
            Show(message);
    }
}
