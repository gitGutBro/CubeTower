using UnityEngine;

namespace CubeTower.Application.Config
{
    /// <summary>
    /// Holds the configuration loaded at startup, so systems do not load it again.
    /// </summary>
    public interface IGameConfigStore
    {
        GameConfig Current { get; }

        bool TryGetColor(string colorId, out Color color);
        bool TryGetDefinition(string colorId, out CubeColorDefinition definition);
    }
}
