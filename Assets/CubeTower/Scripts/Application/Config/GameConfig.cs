using System;
using System.Collections.Generic;
using UnityEngine;

namespace CubeTower.Application.Config
{
    /// <summary>
    /// Game configuration in a source-independent form. It can be built from a ScriptableObject,
    /// a JSON file, a remote config etc. - the game only sees this class.
    /// </summary>
    public sealed class GameConfig
    {
        public GameConfig(int cubeCount, IReadOnlyList<CubeColorDefinition> colors)
        {
            CubeCount = cubeCount;
            Colors = colors ?? throw new ArgumentNullException(nameof(colors));
        }

        /// <summary>Number of cubes in the bottom scroll.</summary>
        public int CubeCount { get; }

        /// <summary>Colors of the cubes in the bottom scroll, one per cube.</summary>
        public IReadOnlyList<CubeColorDefinition> Colors { get; }

        /// <summary>How many cubes the palette can actually show (protects against a count/colors mismatch).</summary>
        public int PaletteSize => Math.Max(0, Math.Min(CubeCount, Colors.Count));
    }

    public sealed class CubeColorDefinition
    {
        public CubeColorDefinition(string id, Color color, Sprite sprite = null)
        {
            Id = id;
            Color = color;
            Sprite = sprite;
        }

        /// <summary>Stable identifier stored in the save file. Changing a color keeps saves valid.</summary>
        public string Id { get; }

        /// <summary>Color of the cube. Used as a tint when there is no dedicated sprite.</summary>
        public Color Color { get; }

        /// <summary>Optional ready-made sprite of this cube. When set, it is shown without tinting.</summary>
        public Sprite Sprite { get; }
    }
}
