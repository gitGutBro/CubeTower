using System;
using System.Collections.Generic;
using UnityEngine;

namespace CubeTower.Application.Config
{
    public sealed class GameConfigStore : IGameConfigStore
    {
        private readonly Dictionary<string, CubeColorDefinition> _definitions = new();
        private GameConfig _current;

        public GameConfig Current =>
            _current ?? throw new InvalidOperationException("The game config has not been loaded yet.");

        public void Set(GameConfig config)
        {
            _current = config ?? throw new ArgumentNullException(nameof(config));
            _definitions.Clear();

            foreach (CubeColorDefinition definition in config.Colors)
                if (definition != null && !string.IsNullOrEmpty(definition.Id))
                    _definitions[definition.Id] = definition;
        }

        public bool TryGetColor(string colorId, out Color color)
        {
            if (TryGetDefinition(colorId, out CubeColorDefinition definition))
            {
                color = definition.Color;
                return true;
            }

            color = default;
            return false;
        }

        public bool TryGetDefinition(string colorId, out CubeColorDefinition definition)
        {
            if (colorId != null)
                return _definitions.TryGetValue(colorId, out definition);

            definition = null;
            return false;
        }
    }
}
