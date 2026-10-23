using DG.Tweening;
using UnityEngine;

namespace CubeTower.Presentation.Settings
{
    /// <summary>
    /// All animation timings in one asset: designers tune the feel without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "CubeTower/Animation Settings", fileName = "AnimationSettings")]
    public sealed class AnimationSettings : ScriptableObject
    {
        [Header("Tower")]
        [SerializeField, Min(0.01f)] private float _jumpDuration = 0.4f;
        [SerializeField, Min(0f), Tooltip("Jump height in cube sizes.")] private float _jumpHeight = 1.2f;
        [SerializeField, Min(0.01f)] private float _moveDuration = 0.25f;
        [SerializeField] private Ease _moveEase = Ease.OutQuad;
        [SerializeField, Min(0.01f)] private float _popInDuration = 0.2f;

        [Header("Disappear")]
        [SerializeField, Min(0.01f)] private float _explodeDuration = 0.4f;
        [SerializeField, Min(0.01f)] private float _fallIntoHoleDuration = 0.5f;

        [Header("Message")]
        [SerializeField, Min(0.01f)] private float _messageFadeInDuration = 0.15f;
        [SerializeField, Min(0f)] private float _messageHoldDuration = 1.6f;
        [SerializeField, Min(0.01f)] private float _messageFadeOutDuration = 0.35f;

        public float JumpDuration => _jumpDuration;
        public float JumpHeight => _jumpHeight;
        public float MoveDuration => _moveDuration;
        public Ease MoveEase => _moveEase;
        public float PopInDuration => _popInDuration;
        public float ExplodeDuration => _explodeDuration;
        public float FallIntoHoleDuration => _fallIntoHoleDuration;
        public float MessageFadeInDuration => _messageFadeInDuration;
        public float MessageHoldDuration => _messageHoldDuration;
        public float MessageFadeOutDuration => _messageFadeOutDuration;
    }
}
