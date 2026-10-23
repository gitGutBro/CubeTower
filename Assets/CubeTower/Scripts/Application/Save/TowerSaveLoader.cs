using System.Linq;
using CubeTower.Application.Config;
using CubeTower.Core.Common;
using CubeTower.Core.Save;
using CubeTower.Core.Tower;

namespace CubeTower.Application.Save
{
    /// <summary>
    /// Saves and restores the tower. The tower decides what its state is
    /// (<see cref="ITower.CreateData"/> / <see cref="ITower.Restore"/>), the loader only
    /// connects it with the repository and checks the data against the current config.
    /// </summary>
    public sealed class TowerSaveLoader : SaveLoader<ITower, TowerData>
    {
        private readonly IGameConfigStore _config;
        private readonly ILog _log;

        public TowerSaveLoader(ITower tower, IGameConfigStore config, ILog log) : base(tower)
        {
            _config = config;
            _log = log;
        }

        protected override string Key => SaveKeys.Tower;

        protected override TowerData ConvertToData(ITower tower) => 
            tower.CreateData();

        protected override bool TrySetupData(ITower tower, TowerData data)
        {
            if (data.Cubes != null && data.Cubes.Any(cube => cube == null || !_config.TryGetColor(cube.ColorId, out _)))
            {
                _log.Warning("Saved tower uses colors that are not in the config any more. The tower is reset.");
                return false;
            }

            if (tower.Restore(data))
                return true;

            _log.Warning("Saved tower data is invalid. The tower is reset.");
            return false;
        }

        protected override void SetupDefaultData(ITower tower) => 
            tower.Clear();
    }
}
