using UnityEngine;

namespace CubeTower.Composition.Boot
{
    /// <summary>
    /// Remembers whether the game was started through the bootstrap scene.
    /// Static state is reset on every play session, so it also works with Domain Reload disabled.
    /// </summary>
    public static class BootstrapFlag
    {
        public static bool IsBootstrapped { get; private set; }

        public static void MarkBootstrapped() => 
            IsBootstrapped = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlaySession() => 
            IsBootstrapped = false;
    }
}
