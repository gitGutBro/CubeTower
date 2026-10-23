using System;
using System.Threading;
using CubeTower.Application.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace CubeTower.Composition.Boot
{
    /// <summary>
    /// Loads a scene and runs its SceneContext. Game scenes have "Auto Run" disabled:
    /// they start only after the bootstrap prepared config and save, never on their own.
    /// </summary>
    public sealed class SceneLoader : ISceneLoader
    {
        public async UniTask LoadAsync(string sceneName, CancellationToken cancellationToken)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)
                .ToUniTask(cancellationToken: cancellationToken);

            Scene scene = SceneManager.GetSceneByName(sceneName);
            SceneContext context = FindSceneContext(scene);

            if (context == null)
                throw new InvalidOperationException($"Scene '{sceneName}' has no SceneContext.");

            if (!context.HasInstalled)
                context.Run();
        }

        private static SceneContext FindSceneContext(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                SceneContext context = root.GetComponentInChildren<SceneContext>(true);
                
                if (context)
                    return context;
            }

            return null;
        }
    }
}
