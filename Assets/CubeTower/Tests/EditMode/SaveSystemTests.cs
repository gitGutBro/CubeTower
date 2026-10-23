using System.Collections.Generic;
using System.Threading;
using CubeTower.Application.Config;
using CubeTower.Application.Save;
using CubeTower.Core.Save;
using CubeTower.Core.Tower;
using CubeTower.Infrastructure.Save;
using NUnit.Framework;
using UnityEngine;

namespace CubeTower.Tests.EditMode
{
    public sealed class SaveSystemTests
    {
        private SilentLog _log;
        private GameConfigStore _config;

        [SetUp]
        public void SetUp()
        {
            _log = new SilentLog();
            _config = new GameConfigStore();
            _config.Set(new GameConfig(3, new[]
            {
                new CubeColorDefinition("red", Color.red),
                new CubeColorDefinition("green", Color.green),
                new CubeColorDefinition("blue", Color.blue),
            }));
        }

        [Test]
        public void Tower_SurvivesSaveAndLoad_ThroughRepositoryAndManager()
        {
            MemoryStateSaver saver = new();
            Tower tower = TowerFactory.Create(0.25f, -0.25f);
            TowerFactory.PlaceOnTop(tower, "red");
            TowerFactory.PlaceOnTop(tower, "green");
            TowerFactory.PlaceOnTop(tower, "blue");
            CreateManager(tower, saver).SaveGameImmediate();

            Tower loaded = TowerFactory.Create();
            CreateManager(loaded, saver).LoadGameAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(loaded.Count, Is.EqualTo(3));
            
            for (int i = 0; i < 3; i++)
            {
                Assert.That(loaded.Cubes[i].Id, Is.EqualTo(tower.Cubes[i].Id));
                Assert.That(loaded.Cubes[i].ColorId, Is.EqualTo(tower.Cubes[i].ColorId));
                Assert.That(loaded.Cubes[i].X, Is.EqualTo(tower.Cubes[i].X).Within(0.0001f));
            }
        }

        [Test]
        public void SavedTowerWithUnknownColor_IsReset()
        {
            MemoryStateSaver saver = new();
            Tower tower = TowerFactory.Create();
            TowerFactory.PlaceOnTop(tower, "purple");
            CreateManager(tower, saver).SaveGameImmediate();

            Tower loaded = TowerFactory.Create();
            TowerFactory.PlaceOnTop(loaded, "red");
            CreateManager(loaded, saver).LoadGameAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(loaded.Count, Is.EqualTo(0));
            Assert.That(_log.Warnings, Is.Not.Empty);
        }

        [Test]
        public void CorruptedEntry_DoesNotBreakLoading()
        {
            MemoryStateSaver saver = new();
            saver.SaveImmediate(new Dictionary<string, string> { [SaveKeys.Tower] = "{ not json" });

            Tower loaded = TowerFactory.Create();
            CreateManager(loaded, saver).LoadGameAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(loaded.Count, Is.EqualTo(0));
            Assert.That(_log.Errors, Is.Not.Empty);
        }

        [Test]
        public void Repository_KeepsEntriesIndependent()
        {
            GameRepository repository = new(_log);
            repository.SetData("a", new TowerCubeData("1", "red", 1f));
            repository.ImportState(new Dictionary<string, string>
            {
                ["a"] = repository.ExportState()["a"],
                ["b"] = "broken",
            });

            Assert.That(repository.TryGetData("a", out TowerCubeData data), Is.True);
            Assert.That(data.ColorId, Is.EqualTo("red"));
            Assert.That(repository.TryGetData("b", out TowerCubeData _), Is.False);
        }

        private SaveLoadManager CreateManager(Tower tower, IGameStateSaver saver)
        {
            ISaveLoader[] loaders = new ISaveLoader[] { new TowerSaveLoader(tower, _config, _log) };
            return new SaveLoadManager(loaders, new GameRepository(_log), saver, _log);
        }
    }
}
