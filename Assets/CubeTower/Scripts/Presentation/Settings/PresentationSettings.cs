using UnityEngine;

namespace CubeTower.Presentation.Settings
{
    /// <summary>
    /// Visual and input settings of the game scene.
    /// </summary>
    [CreateAssetMenu(menuName = "CubeTower/Presentation Settings", fileName = "PresentationSettings")]
    public sealed class PresentationSettings : ScriptableObject
    {
        [Header("Cubes")]
        [SerializeField] private Sprite _cubeSprite;
        [SerializeField, Range(0.3f, 1f), Tooltip("Cube size as a part of the bottom scroll height.")]
        private float _cubeSizeToPaletteHeight = 0.7f;
        [SerializeField, Range(0f, 1f), Tooltip("Spacing between cubes in the scroll, in cube sizes.")]
        private float _paletteSpacing = 0.25f;

        [Header("Input")]
        [SerializeField, Range(10f, 80f), Tooltip(
            "A drag that starts in the bottom scroll is a scroll if its angle to the horizontal " +
            "is below this value, otherwise it picks up a cube.")]
        private float _scrollMaxAngle = 45f;

        public Sprite CubeSprite => _cubeSprite;
        public float CubeSizeToPaletteHeight => _cubeSizeToPaletteHeight;
        public float PaletteSpacing => _paletteSpacing;
        public float ScrollMaxAngle => _scrollMaxAngle;

#if UNITY_EDITOR
        public void EditorSetup(Sprite cubeSprite) => 
            _cubeSprite = cubeSprite;
#endif
    }
}
