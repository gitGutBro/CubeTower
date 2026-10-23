using System.IO;

namespace CubeTower.Infrastructure.Save
{
    /// <summary>
    /// Location of the save file. Must be resolved on the main thread:
    /// <c>Application.persistentDataPath</c> is a Unity API.
    /// </summary>
    public static class SaveFileLocation
    {
        public const string FileName = "cubetower_save.dat";
        
        public static string DefaultPath => Path.Combine(UnityEngine.Application.persistentDataPath, FileName);
    }
}
