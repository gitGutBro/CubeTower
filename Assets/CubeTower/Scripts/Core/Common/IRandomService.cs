namespace CubeTower.Core.Common
{
    /// <summary>
    /// Source of randomness. Hidden behind an interface so tests can make it deterministic.
    /// </summary>
    public interface IRandomService
    {
        /// <summary>Returns a random value in the inclusive range [min, max].</summary>
        float Range(float min, float max);
    }
}
