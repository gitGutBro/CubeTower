using System;
using CubeTower.Core.Common;
using UnityEngine;

namespace CubeTower.Infrastructure.Logging
{
    /// <summary>
    /// <see cref="ILog"/> over the Unity console. Debug.Log is thread-safe,
    /// so background save code may log too.
    /// </summary>
    public sealed class UnityLog : ILog
    {
        private const string Prefix = "[CubeTower] ";

        public void Info(string message) => 
            Debug.Log(Prefix + message);

        public void Warning(string message) => 
            Debug.LogWarning(Prefix + message);

        public void Error(string message, Exception exception = null)
        {
            Debug.LogError(Prefix + message);

            if (exception != null)
                Debug.LogException(exception);
        }
    }
}
