namespace CubeTower.Core.Save
{
    /// <summary>
    /// A dictionary "key -> serialized object". Every system writes its own entry
    /// and reads only its own entry, so a broken entry does not break the others.
    /// </summary>
    public interface IGameRepository
    {
        bool TryGetData<T>(string key, out T data);
        void SetData<T>(string key, T data);
        void RemoveData(string key);
    }
}
