using System;
using System.Collections.Generic;
using System.Threading;
using CubeTower.Application.Save;
using CubeTower.Core.Common;
using CubeTower.Core.Tower;
using CubeTower.Core.Tower.Rules;
using Cysharp.Threading.Tasks;

namespace CubeTower.Tests.EditMode
{
    internal sealed class FixedRandom : IRandomService
    {
        private readonly Queue<float> _values;

        public FixedRandom(params float[] values) => 
            _values = new Queue<float>(values);

        public float Range(float min, float max)
        {
            float value = _values.Count > 0 ? _values.Dequeue() : 0f;
            return Math.Max(min, Math.Min(max, value));
        }
    }

    internal sealed class SequentialIds : IIdGenerator
    {
        private int _next;

        public string Next() => 
            $"cube{++_next}";
    }

    internal sealed class SilentLog : ILog
    {
        public List<string> Warnings { get; } = new();
        public List<string> Errors { get; } = new();

        public void Info(string message)
        {
            
        }
        
        public void Warning(string message) => 
            Warnings.Add(message);
        
        public void Error(string message, Exception exception = null) => 
            Errors.Add(message);
    }

    /// <summary>Keeps the "file" in memory; completes synchronously.</summary>
    internal sealed class MemoryStateSaver : IGameStateSaver
    {
        public IReadOnlyDictionary<string, string> Stored { get; private set; } = new Dictionary<string, string>();
        public int SaveCount { get; private set; }

        public UniTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken) =>
            UniTask.FromResult(Stored);

        public UniTask SaveAsync(IReadOnlyDictionary<string, string> state, CancellationToken cancellationToken)
        {
            SaveImmediate(state);
            return UniTask.CompletedTask;
        }

        public void SaveImmediate(IReadOnlyDictionary<string, string> state)
        {
            Dictionary<string, string> copy = new();
            
            foreach (KeyValuePair<string, string> entry in state)
                copy[entry.Key] = entry.Value;

            Stored = copy;
            SaveCount++;
        }
    }

    internal static class TowerFactory
    {
        public const float AreaWidth = 6f;
        public const float AreaHeight = 10f;

        public static Tower Create(params float[] offsets)
        {
            return new Tower(
                new IPlacementRule[] { new DropOverTowerRule(), new HeightLimitRule() },
                new FixedRandom(offsets),
                new SequentialIds());
        }

        public static PlacementRequest DropAt(float x, float y, string color = "red") =>
            new(color, x, y, AreaWidth, AreaHeight);

        /// <summary>Drops a cube right above the current top cube.</summary>
        public static PlacementResult PlaceOnTop(Tower tower, string color = "red")
        {
            if (tower.Count == 0)
                return tower.TryPlace(DropAt(AreaWidth / 2f, 0.5f, color));

            TowerCube top = tower.Cubes[tower.Count - 1];
            return tower.TryPlace(DropAt(top.X, tower.TopEdge + 0.5f, color));
        }
    }
}
