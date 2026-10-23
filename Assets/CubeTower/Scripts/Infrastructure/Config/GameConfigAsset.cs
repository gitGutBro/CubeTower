using System;
using System.Collections.Generic;
using System.Linq;
using CubeTower.Application.Config;
using UnityEngine;

namespace CubeTower.Infrastructure.Config
{
    /// <summary>
    /// ScriptableObject source of the game config: cube count and their colors.
    /// The game never reads this asset directly - it goes through <see cref="IGameConfigProvider"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "CubeTower/Game Config", fileName = "GameConfig")]
    public sealed class GameConfigAsset : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Number of cubes in the bottom scroll.")]
        private int _cubeCount = 20;

        [SerializeField]
        [Tooltip("Color of every cube. The id is stored in save files, keep it stable.")]
        private List<CubeColorEntry> _colors = new List<CubeColorEntry>();

        public int CubeCount => _cubeCount;
        public IReadOnlyList<CubeColorEntry> Colors => _colors;

        public GameConfig ToConfig()
        {
            CubeColorDefinition[] colors = _colors
                .Where(entry => entry != null)
                .Select(entry => new CubeColorDefinition(entry.Id, entry.Color, entry.Sprite))
                .ToArray();

            return new GameConfig(_cubeCount, colors);
        }

#if UNITY_EDITOR
        /// <summary>Used by the scene builder to fill a freshly created asset with defaults.</summary>
        public void EditorSetup(int cubeCount, IEnumerable<CubeColorEntry> colors)
        {
            _cubeCount = cubeCount;
            _colors = colors.ToList();
        }
#endif
    }

    [Serializable]
    public sealed class CubeColorEntry
    {
        [SerializeField]
        private string _id;

        [SerializeField]
        private Color _color = Color.white;

        [SerializeField]
        [Tooltip("Optional ready-made sprite. If empty, the default cube sprite is tinted with the color.")]
        private Sprite _sprite;

        public CubeColorEntry() { }

        public CubeColorEntry(string id, Color color, Sprite sprite = null)
        {
            _id = id;
            _color = color;
            _sprite = sprite;
        }

        public string Id => _id;
        public Color Color => _color;
        public Sprite Sprite => _sprite;
    }
}
