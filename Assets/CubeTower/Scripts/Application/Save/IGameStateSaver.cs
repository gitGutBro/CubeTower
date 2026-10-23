using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace CubeTower.Application.Save
{
    /// <summary>
    /// Where and how the whole game state is stored.
    /// Implementations: a JSON file with optional AES encryption. A server or cloud saver
    /// would be another implementation of the same interface.
    /// </summary>
    public interface IGameStateSaver
    {
        /// <summary>Loads the state. Never throws on a missing or broken file: returns an empty state.</summary>
        UniTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken);

        /// <summary>Saves the state; the IO part runs on a background thread.</summary>
        UniTask SaveAsync(IReadOnlyDictionary<string, string> state, CancellationToken cancellationToken);

        /// <summary>Saves synchronously on the calling thread. Used when the app is paused or closed.</summary>
        void SaveImmediate(IReadOnlyDictionary<string, string> state);
    }
}
