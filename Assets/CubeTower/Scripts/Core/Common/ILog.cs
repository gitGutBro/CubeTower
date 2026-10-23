using System;

namespace CubeTower.Core.Common
{
    /// <summary>
    /// Logging port. The core cannot reference UnityEngine (noEngineReferences),
    /// so logging goes through an abstraction implemented in the infrastructure layer.
    /// </summary>
    public interface ILog
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception exception = null);
    }
}
