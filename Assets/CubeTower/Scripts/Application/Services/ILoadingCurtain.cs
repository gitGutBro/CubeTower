namespace CubeTower.Application.Services
{
    /// <summary>
    /// Loading screen that covers the game while it starts.
    /// </summary>
    public interface ILoadingCurtain
    {
        void Show(string message);
        void Hide();

        /// <summary>Shows a startup error. The curtain stays visible: the game cannot continue.</summary>
        void ShowError(string message);
    }
}
