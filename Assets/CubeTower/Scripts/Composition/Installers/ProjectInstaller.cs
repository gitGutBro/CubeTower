using CubeTower.Infrastructure.Config;
using CubeTower.Infrastructure.Localization;
using CubeTower.Presentation.Views;
using UnityEngine;
using Zenject;

namespace CubeTower.Composition.Installers
{
    /// <summary>
    /// Installer of the ProjectContext (Resources/ProjectContext.prefab): everything that lives
    /// for the whole application - model, config, localization, saves, the state machine.
    /// Bindings are split into thematic sub-installers so this class stays small.
    /// </summary>
    public sealed class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private GameConfigAsset _gameConfig;
        [SerializeField] private LocalizationTableAsset _localizationTable;
        [SerializeField] private LoadingCurtainView _curtainPrefab;

        [Header("Saves")]
        [SerializeField, Tooltip("Encrypt the save file with AES-256 + HMAC. Disable to read it as plain JSON.")]
        private bool _encryptSaves = true;

        [SerializeField, Min(0f), Tooltip("Delay after the last change before the game is saved.")]
        private float _autoSaveDelaySeconds = 0.5f;

        public override void InstallBindings()
        {
            InfrastructureInstaller.Install(Container, _gameConfig, _localizationTable);
            TowerInstaller.Install(Container);
            SaveInstaller.Install(Container, _encryptSaves, _autoSaveDelaySeconds);
            StateMachineInstaller.Install(Container, _curtainPrefab);
        }
    }
}
