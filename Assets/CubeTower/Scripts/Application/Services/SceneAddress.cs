namespace CubeTower.Application.Services
{
    /// <summary>
    /// Scene names in one place, no magic strings.
    /// Names are prefixed to avoid clashes with scenes of the host project.
    /// </summary>
    public static class SceneAddress
    {
        public const string Bootstrap = "CubeTowerBootstrap";
        public const string Game = "CubeTowerGame";
    }
}
