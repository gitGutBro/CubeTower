namespace CubeTower.Application.Localization
{
    /// <summary>
    /// Localization port. The game asks for texts by key and never contains hardcoded strings,
    /// so a real localization system (Unity Localization, I2 etc.) can replace the implementation.
    /// </summary>
    public interface ILocalizationService
    {
        string Language { get; }

        string Get(string key);
        string Get(string key, params object[] arguments);
    }
}
