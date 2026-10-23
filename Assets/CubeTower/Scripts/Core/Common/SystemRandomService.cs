using System;

namespace CubeTower.Core.Common
{
    /// <summary>
    /// <see cref="IRandomService"/> backed by <see cref="System.Random"/>.
    /// Not thread-safe: the game calls it from the main thread only.
    /// </summary>
    public sealed class SystemRandomService : IRandomService
    {
        private readonly Random _random;

        public SystemRandomService() : this(Environment.TickCount) { }

        public SystemRandomService(int seed) => 
            _random = new Random(seed);

        public float Range(float min, float max)
        {
            if (min > max)
                throw new ArgumentException($"{nameof(min)} must not be greater than {nameof(max)}.");

            return min + ((float)_random.NextDouble() * (max - min));
        }
    }
}
