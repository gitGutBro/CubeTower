using System;
using System.Collections.Generic;
using CubeTower.Application.Save;
using CubeTower.Core.Common;
using Newtonsoft.Json;

namespace CubeTower.Infrastructure.Save
{
    /// <summary>
    /// A dictionary "key -> JSON". Every entry is serialized separately,
    /// so a corrupted entry only affects its own system and is deserialized lazily on request.
    /// Lives on the main thread; the save manager exports a copy for background writing.
    /// </summary>
    public sealed class GameRepository : IGameStateRepository
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
        };

        private readonly Dictionary<string, string> _state = new();
        private readonly ILog _log;

        public GameRepository(ILog log) => 
            _log = log ?? throw new ArgumentNullException(nameof(log));

        public bool TryGetData<T>(string key, out T data)
        {
            data = default;

            if (string.IsNullOrEmpty(key) || !_state.TryGetValue(key, out string json))
                return false;

            try
            {
                data = JsonConvert.DeserializeObject<T>(json, SerializerSettings);
                return data != null;
            }
            catch (JsonException exception)
            {
                _log.Error($"Save entry '{key}' is corrupted and is ignored.", exception);
                return false;
            }
        }

        public void SetData<T>(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Save key must not be empty.", nameof(key));

            _state[key] = JsonConvert.SerializeObject(data, SerializerSettings);
        }

        public void RemoveData(string key)
        {
            if (!string.IsNullOrEmpty(key))
                _state.Remove(key);
        }

        public IReadOnlyDictionary<string, string> ExportState() => 
            new Dictionary<string, string>(_state);

        public void ImportState(IReadOnlyDictionary<string, string> state)
        {
            _state.Clear();

            if (state == null)
                return;

            foreach (KeyValuePair<string, string> entry in state)
                if (!string.IsNullOrEmpty(entry.Key) && entry.Value != null)
                    _state[entry.Key] = entry.Value;
        }
    }
}
