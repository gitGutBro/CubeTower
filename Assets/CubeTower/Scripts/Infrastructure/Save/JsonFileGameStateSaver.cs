using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CubeTower.Application.Save;
using CubeTower.Core.Common;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace CubeTower.Infrastructure.Save
{
    /// <summary>
    /// Stores the whole game state in one file: JSON -> protector (AES) -> disk.
    /// <list type="bullet">
    /// <item>IO and crypto run on the thread pool; only an immutable copy of the state goes there.</item>
    /// <item>Writes are atomic: a temp file is written and flushed, then replaces the old file,
    /// so a crash in the middle of a write never leaves a half-written save.</item>
    /// <item>A lock serializes background and synchronous (on quit) writes.</item>
    /// <item>A broken file is moved to *.bak for investigation; the game starts with an empty state.</item>
    /// </list>
    /// </summary>
    public sealed class JsonFileGameStateSaver : IGameStateSaver
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyState = new Dictionary<string, string>();

        private readonly ISaveDataProtector _protector;
        private readonly ILog _log;
        private readonly string _filePath;
        private readonly object _fileLock = new();

        public JsonFileGameStateSaver(ISaveDataProtector protector, ILog log, string filePath)
        {
            _protector = protector ?? throw new ArgumentNullException(nameof(protector));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Save file path must not be empty.", nameof(filePath));

            _filePath = filePath;
        }

        public string FilePath => _filePath;

        public UniTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken) => 
            UniTask.RunOnThreadPool(Load, cancellationToken: cancellationToken);

        public UniTask SaveAsync(IReadOnlyDictionary<string, string> state, CancellationToken cancellationToken)
        {
            Dictionary<string, string> snapshot = Copy(state);
            return UniTask.RunOnThreadPool(() => Write(snapshot), cancellationToken: cancellationToken);
        }

        public void SaveImmediate(IReadOnlyDictionary<string, string> state) => 
            Write(Copy(state));

        /// <summary>Reads and decrypts the file. Public for editor debug tools.</summary>
        public IReadOnlyDictionary<string, string> Load()
        {
            lock (_fileLock)
            {
                if (!File.Exists(_filePath))
                    return EmptyState;

                try
                {
                    byte[] protectedBytes = File.ReadAllBytes(_filePath);

                    if (!_protector.TryUnprotect(protectedBytes, out byte[] bytes))
                    {
                        BackupBrokenFile("the file is modified, corrupted or encrypted with another key");
                        return EmptyState;
                    }

                    string json = Encoding.UTF8.GetString(bytes);
                    Dictionary<string, string> state = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    return state ?? EmptyState;
                }
                catch (JsonException exception)
                {
                    BackupBrokenFile("the JSON is invalid: " + exception.Message);
                    return EmptyState;
                }
                catch (IOException exception)
                {
                    _log.Error("Failed to read the save file. Starting with an empty state.", exception);
                    return EmptyState;
                }
                catch (UnauthorizedAccessException exception)
                {
                    _log.Error("No access to the save file. Starting with an empty state.", exception);
                    return EmptyState;
                }
            }
        }

        private void Write(Dictionary<string, string> state)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(state));
            byte[] protectedBytes = _protector.Protect(bytes);

            lock (_fileLock)
            {
                string directory = Path.GetDirectoryName(_filePath);
                
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                string tempPath = _filePath + ".tmp";

                try
                {
                    using (FileStream stream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        stream.Write(protectedBytes, 0, protectedBytes.Length);
                        stream.Flush(true);
                    }

                    ReplaceFile(tempPath, _filePath);
                }
                finally
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
            }
        }

        private static void ReplaceFile(string source, string destination)
        {
            if (!File.Exists(destination))
            {
                File.Move(source, destination);
                return;
            }

            try
            {
                File.Replace(source, destination, null);
            }
            catch (PlatformNotSupportedException)
            {
                File.Copy(source, destination, overwrite: true);
                File.Delete(source);
            }
        }

        private void BackupBrokenFile(string reason)
        {
            string backupPath = $"{_filePath}.{DateTime.UtcNow:yyyyMMddHHmmss}.bak";
            
            try
            {
                File.Move(_filePath, backupPath);
                _log.Warning($"Save file is broken ({reason}). Moved to '{backupPath}'. Starting with an empty state.");
            }
            catch (IOException exception)
            {
                _log.Error($"Save file is broken ({reason}) and could not be moved to a backup.", exception);
            }
        }

        private static Dictionary<string, string> Copy(IReadOnlyDictionary<string, string> state)
        {
            Dictionary<string, string> copy = new();

            if (state == null)
                return copy;

            foreach (KeyValuePair<string, string> entry in state)
                copy[entry.Key] = entry.Value;

            return copy;
        }
    }
}
