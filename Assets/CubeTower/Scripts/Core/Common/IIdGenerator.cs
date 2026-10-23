namespace CubeTower.Core.Common
{
    /// <summary>
    /// Generates unique identifiers for runtime entities (cubes in the tower).
    /// </summary>
    public interface IIdGenerator
    {
        string Next();
    }
}
