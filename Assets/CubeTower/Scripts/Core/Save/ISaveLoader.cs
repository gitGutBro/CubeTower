namespace CubeTower.Core.Save
{
    /// <summary>
    /// Bridge between one game system and the repository.
    /// The save manager calls all loaders: on save they put data into the repository,
    /// on load they take data from it and set the system up.
    /// </summary>
    public interface ISaveLoader
    {
        void SaveGame(IGameRepository repository);
        void LoadGame(IGameRepository repository);
    }
}
