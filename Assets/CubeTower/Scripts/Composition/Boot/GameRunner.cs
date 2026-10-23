using CubeTower.Application.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CubeTower.Composition.Boot
{
    /// <summary>
    /// Lets you press Play in the game scene.
    /// If the game was not started through the bootstrap scene, it loads the bootstrap scene.
    /// The game scene's SceneContext has "Auto Run" disabled, so nothing in it starts by itself:
    /// it is started by the scene loader after bootstrap.
    /// </summary>
    public sealed class GameRunner : MonoBehaviour
    {
        private void Awake()
        {
            if (!BootstrapFlag.IsBootstrapped)
                SceneManager.LoadScene(SceneAddress.Bootstrap);
        }
    }
}
