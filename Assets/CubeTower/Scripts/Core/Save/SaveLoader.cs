using System;

namespace CubeTower.Core.Save
{
    /// <summary>
    /// Converts a service to a DTO and back.
    /// The service itself knows nothing about saving.
    /// Difference from the course version: the service is injected through the constructor
    /// (instead of a service locator), and the key is an explicit stable string
    /// (instead of typeof(T).Name, which silently breaks saves when a class is renamed).
    /// </summary>
    public abstract class SaveLoader<TService, TData> : ISaveLoader
        where TData : class
    {
        protected SaveLoader(TService service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            Service = service;
        }

        protected TService Service { get; }

        /// <summary>Stable key of the entry in the repository. Never derive it from a type name.</summary>
        protected abstract string Key { get; }

        void ISaveLoader.SaveGame(IGameRepository repository) => 
            repository.SetData(Key, ConvertToData(Service));

        void ISaveLoader.LoadGame(IGameRepository repository)
        {
            if (repository.TryGetData(Key, out TData data) && data != null && TrySetupData(Service, data))
                return;

            SetupDefaultData(Service);
        }

        protected abstract TData ConvertToData(TService service);

        /// <summary>Applies loaded data. Returns false if the data is invalid; then defaults are used.</summary>
        protected abstract bool TrySetupData(TService service, TData data);

        /// <summary>Called when there is no saved data, or it is invalid.</summary>
        protected virtual void SetupDefaultData(TService service)
        {
        }
    }
}
