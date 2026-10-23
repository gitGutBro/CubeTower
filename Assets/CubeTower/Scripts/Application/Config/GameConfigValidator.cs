using System.Collections.Generic;
using UnityEngine;

namespace CubeTower.Application.Config
{
    /// <summary>
    /// Checks a configuration. Used at startup (errors stop the game with a clear message)
    /// and by editor validators (errors are highlighted in the inspector).
    /// </summary>
    public static class GameConfigValidator
    {
        public static ConfigValidationReport Validate(GameConfig config)
        {
            var report = new ConfigValidationReport();

            if (config == null)
            {
                report.Errors.Add("Config is missing.");
                return report;
            }

            if (config.CubeCount <= 0)
                report.Errors.Add($"Cube count must be positive (now {config.CubeCount}).");

            if (config.Colors.Count == 0)
            {
                report.Errors.Add("Color list is empty.");
                return report;
            }

            if (config.Colors.Count != config.CubeCount)
            {
                report.Warnings.Add(
                    $"Cube count ({config.CubeCount}) differs from colors count ({config.Colors.Count}). " +
                    $"The palette will show {config.PaletteSize} cubes.");
            }

            HashSet<string> ids = new();
            HashSet<Color32> colors = new();

            for (int i = 0; i < config.Colors.Count; i++)
            {
                CubeColorDefinition definition = config.Colors[i];

                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                {
                    report.Errors.Add($"Color #{i} has an empty id.");
                    continue;
                }

                if (!ids.Add(definition.Id))
                    report.Errors.Add($"Color id '{definition.Id}' is used more than once.");

                if (!colors.Add(definition.Color))
                    report.Warnings.Add($"Color '{definition.Id}' duplicates another color: cubes must have different colors.");

                if (definition.Color.a < 1f)
                    report.Warnings.Add($"Color '{definition.Id}' is transparent.");
            }

            return report;
        }
    }

    public sealed class ConfigValidationReport
    {
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public bool IsValid => Errors.Count == 0;
    }
}
