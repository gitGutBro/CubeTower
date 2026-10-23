using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CubeTower.Core.Common;
using CubeTower.Core.Save;
using Cysharp.Threading.Tasks;

namespace CubeTower.Application.Save
{
    /// <summary>
    /// Collects data from all loaders into the repository and writes it,
    /// or reads the repository and hands data back to the loaders.
    /// Plain C# instead of a MonoBehaviour; editor buttons live in the Odin hub.
    /// </summary>
    public sealed class SaveLoadManager : ISaveLoadManager
    {
        private readonly IReadOnlyList<ISaveLoader> _saveLoaders;
        private readonly IGameStateRepository _repository;
        private readonly IGameStateSaver _saver;
        private readonly ILog _log;

        private bool _isSaving;
        private bool _hasPendingSave;

        public SaveLoadManager(
            IEnumerable<ISaveLoader> saveLoaders,
            IGameStateRepository repository,
            IGameStateSaver saver,
            ILog log)
        {
            _saveLoaders = saveLoaders?.ToArray() ?? throw new ArgumentNullException(nameof(saveLoaders));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _saver = saver ?? throw new ArgumentNullException(nameof(saver));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async UniTask LoadGameAsync(CancellationToken cancellationToken)
        {
            IReadOnlyDictionary<string, string> state = await _saver.LoadAsync(cancellationToken);
            _repository.ImportState(state);

            foreach (ISaveLoader saveLoader in _saveLoaders)
            {
                try
                {
                    saveLoader.LoadGame(_repository);
                }
                catch (Exception exception)
                {
                    _log.Error($"Failed to load '{saveLoader.GetType().Name}'. It starts with default data.", exception);
                }
            }
        }

        public async UniTask SaveGameAsync(CancellationToken cancellationToken)
        {
            if (_isSaving)
            {
                _hasPendingSave = true;
                return;
            }

            _isSaving = true;
            try
            {
                do
                {
                    _hasPendingSave = false;
                    IReadOnlyDictionary<string, string> state = CollectState();
                    await _saver.SaveAsync(state, cancellationToken);
                }
                while (_hasPendingSave && !cancellationToken.IsCancellationRequested);
            }
            catch (OperationCanceledException) { }
            catch (Exception exception)
            {
                _log.Error("Failed to save the game.", exception);
            }
            finally
            {
                _isSaving = false;
            }
        }

        public void SaveGameImmediate()
        {
            try
            {
                _hasPendingSave = false;
                _saver.SaveImmediate(CollectState());
            }
            catch (Exception exception)
            {
                _log.Error("Failed to save the game synchronously.", exception);
            }
        }

        private IReadOnlyDictionary<string, string> CollectState()
        {
            foreach (ISaveLoader saveLoader in _saveLoaders)
            {
                try
                {
                    saveLoader.SaveGame(_repository);
                }
                catch (Exception exception)
                {
                    _log.Error($"Failed to collect save data from '{saveLoader.GetType().Name}'.", exception);
                }
            }

            return _repository.ExportState();
        }
    }
}
