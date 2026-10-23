using System;
using System.Collections.Generic;
using System.Linq;
using CubeTower.Core.Tower;
using NUnit.Framework;
using R3;

namespace CubeTower.Tests.EditMode
{
    public sealed class TowerTests
    {
        [Test]
        public void FirstCube_IsPlacedWhereDropped_AndClampedInsideArea()
        {
            Tower tower = TowerFactory.Create();

            PlacementResult result = tower.TryPlace(TowerFactory.DropAt(-3f, 4.5f));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(tower.Cubes[0].X, Is.EqualTo(0.5f), "The cube must be fully inside the area.");
            Assert.That(tower.BaseY, Is.EqualTo(4f).Within(0.0001f));
        }

        [Test]
        public void NextCube_IsShiftedByNoMoreThanHalfOfEdge()
        {
            Tower tower = TowerFactory.Create(0.5f, -0.5f, 0.9f, -7f);

            for (int i = 0; i < 5; i++)
                Assert.That(TowerFactory.PlaceOnTop(tower).IsSuccess, Is.True);

            for (int i = 1; i < tower.Count; i++)
            {
                float offset = Math.Abs(tower.Cubes[i].X - tower.Cubes[i - 1].X);
                Assert.That(offset, Is.LessThanOrEqualTo(TowerGeometry.MaxHorizontalOffset + 0.0001f));
            }
        }

        [Test]
        public void CubeDroppedAwayFromTower_IsRejected()
        {
            Tower tower = TowerFactory.Create();
            TowerFactory.PlaceOnTop(tower);

            PlacementResult result = tower.TryPlace(TowerFactory.DropAt(0.5f, 8f));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.FailReason, Is.EqualTo(PlacementFailReasons.MissedTower));
            Assert.That(tower.Count, Is.EqualTo(1));
        }

        [Test]
        public void CubeDroppedOverAnyPlacedCube_JumpsOnTop()
        {
            Tower tower = TowerFactory.Create(0f, 0f);
            TowerFactory.PlaceOnTop(tower);
            TowerFactory.PlaceOnTop(tower);

            // Dropped over the bottom cube, not over the top one.
            TowerCube bottom = tower.Cubes[0];
            PlacementResult result = tower.TryPlace(TowerFactory.DropAt(bottom.X, tower.BaseY + 0.5f));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(tower.Count, Is.EqualTo(3));
            Assert.That(tower.Cubes[2].Id, Is.EqualTo(result.Cube.Id));
        }

        [Test]
        public void CubeCrossingTheTop_IsPlaced_ThenTowerStopsGrowing()
        {
            Tower tower = TowerFactory.Create();
            tower.TryPlace(new PlacementRequest("red", 1f, 0.5f, 3f, 2.5f));
            TowerFactory.PlaceOnTop(tower);

            // Top edge is 2.0 <= 2.5: one more cube still fits the rule and crosses the border.
            PlacementResult crossing = tower.TryPlace(new PlacementRequest("red", tower.Cubes[1].X, 2.2f, 3f, 2.5f));
            Assert.That(crossing.IsSuccess, Is.True);
            Assert.That(tower.IsHeightLimitReached(2.5f), Is.True);

            PlacementResult blocked = tower.TryPlace(new PlacementRequest("red", tower.Cubes[2].X, 2.6f, 3f, 2.5f));
            Assert.That(blocked.IsSuccess, Is.False);
            Assert.That(blocked.FailReason, Is.EqualTo(PlacementFailReasons.HeightLimit));
        }

        [Test]
        public void RemovingMiddleCube_ShiftsCubesDown_AndKeepsOffsetLimit()
        {
            // Offsets +0.5 and +0.5: after removing the middle cube the top one would be 1.0 away.
            Tower tower = TowerFactory.Create(0.5f, 0.5f);
            TowerFactory.PlaceOnTop(tower);
            TowerFactory.PlaceOnTop(tower);
            TowerFactory.PlaceOnTop(tower);
            string middleId = tower.Cubes[1].Id;

            Assert.That(tower.TryRemove(middleId), Is.True);

            Assert.That(tower.Count, Is.EqualTo(2));
            Assert.That(tower.Cubes.Any(cube => cube.Id == middleId), Is.False);
            float offset = Math.Abs(tower.Cubes[1].X - tower.Cubes[0].X);
            Assert.That(offset, Is.LessThanOrEqualTo(TowerGeometry.MaxHorizontalOffset + 0.0001f));
        }

        [Test]
        public void Events_AreRaised_ForAddRemoveAndRestore()
        {
            Tower tower = TowerFactory.Create();
            List<string> events = new();
            tower.Added.Subscribe(added => events.Add("added:" + added.Index));
            tower.Removed.Subscribe(removed => events.Add("removed:" + removed.Index));
            tower.Reset.Subscribe(_ => events.Add("reset"));

            TowerFactory.PlaceOnTop(tower);
            tower.TryRemove(tower.Cubes[0].Id);
            tower.Restore(new TowerData(TowerData.CurrentVersion, 0f, Array.Empty<TowerCubeData>()));

            Assert.That(events, Is.EqualTo(new[] { "added:0", "removed:0", "reset" }));
        }

        [Test]
        public void ChangingTowerFromSubscriber_IsForbidden()
        {
            Tower tower = TowerFactory.Create();
            Exception caught = null;
            tower.Added.Subscribe(added =>
            {
                try
                {
                    tower.TryRemove(added.Cube.Id);
                }
                catch (InvalidOperationException exception)
                {
                    caught = exception;
                }
            });

            TowerFactory.PlaceOnTop(tower);

            Assert.That(caught, Is.Not.Null);
            Assert.That(tower.Count, Is.EqualTo(1));
        }

        [Test]
        public void CreateData_ThenRestore_GivesTheSameTower()
        {
            Tower source = TowerFactory.Create(0.3f, -0.2f);
            TowerFactory.PlaceOnTop(source, "red");
            TowerFactory.PlaceOnTop(source, "green");
            TowerFactory.PlaceOnTop(source, "blue");

            Tower restored = TowerFactory.Create();
            bool isRestored = restored.Restore(source.CreateData());

            Assert.That(isRestored, Is.True);
            Assert.That(restored.BaseY, Is.EqualTo(source.BaseY));
            Assert.That(restored.Cubes.Select(cube => (cube.Id, cube.ColorId, cube.X)),
                Is.EqualTo(source.Cubes.Select(cube => (cube.Id, cube.ColorId, cube.X))));
        }

        [Test]
        public void Restore_RejectsInvalidData()
        {
            Tower tower = TowerFactory.Create();

            Assert.That(tower.Restore(null), Is.False);
            Assert.That(tower.Restore(new TowerData(99, 0f, Array.Empty<TowerCubeData>())), Is.False, "unknown version");
            Assert.That(tower.Restore(new TowerData(TowerData.CurrentVersion, 0f, new[]
            {
                new TowerCubeData("a", "red", 1f),
                new TowerCubeData("a", "red", 1f),
            })), Is.False, "duplicate ids");
            Assert.That(tower.Restore(new TowerData(TowerData.CurrentVersion, 0f, new[]
            {
                new TowerCubeData("a", "red", 1f),
                new TowerCubeData("b", "red", 2f),
            })), Is.False, "offset above 50%");
            Assert.That(tower.Restore(new TowerData(TowerData.CurrentVersion, float.NaN, Array.Empty<TowerCubeData>())), Is.False, "NaN");
        }

        [Test]
        public void FitToArea_MovesTowerBackOnScreen()
        {
            Tower tower = TowerFactory.Create();
            tower.Restore(new TowerData(TowerData.CurrentVersion, 20f, new[]
            {
                new TowerCubeData("a", "red", 9f),
                new TowerCubeData("b", "red", 9.4f),
            }));

            tower.FitToArea(TowerFactory.AreaWidth, TowerFactory.AreaHeight);

            Assert.That(tower.Cubes.Max(cube => cube.X) + 0.5f, Is.LessThanOrEqualTo(TowerFactory.AreaWidth + 0.0001f));
            Assert.That(tower.BaseY, Is.LessThanOrEqualTo(TowerFactory.AreaHeight - 1f));
            Assert.That(tower.Cubes[1].X - tower.Cubes[0].X, Is.EqualTo(0.4f).Within(0.0001f), "Offsets are preserved.");
        }
    }
}
