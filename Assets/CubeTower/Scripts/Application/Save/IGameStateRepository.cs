using System.Collections.Generic;
using CubeTower.Core.Save;

namespace CubeTower.Application.Save
{
    /// <summary>
    /// Repository plus export/import of the whole state. Systems see only <see cref="IGameRepository"/>;
    /// the save manager additionally moves the whole dictionary to and from the storage.
    /// </summary>
    public interface IGameStateRepository : IGameRepository
    {
        /// <summary>Returns a copy, so the caller can hand it to a background thread safely.</summary>
        IReadOnlyDictionary<string, string> ExportState();
        void ImportState(IReadOnlyDictionary<string, string> state);
    }
}
