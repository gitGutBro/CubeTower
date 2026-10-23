using CubeTower.Core.Tower;
using CubeTower.Core.Tower.Rules;
using NUnit.Framework;

namespace CubeTower.Tests.EditMode
{
    public sealed class PlacementRulesTests
    {
        [Test]
        public void SameColorRule_ShowsHowToExtendTheGame()
        {
            // The extension from the task ("only on a cube of the same color") is one new rule class.
            Tower tower = new Tower(
                new IPlacementRule[] { new DropOverTowerRule(), new HeightLimitRule(), new SameColorAsTopRule() },
                new FixedRandom(),
                new SequentialIds());

            TowerFactory.PlaceOnTop(tower, "red");

            PlacementResult wrongColor = TowerFactory.PlaceOnTop(tower, "blue");
            PlacementResult sameColor = TowerFactory.PlaceOnTop(tower, "red");

            Assert.That(wrongColor.IsSuccess, Is.False);
            Assert.That(wrongColor.FailReason, Is.EqualTo(PlacementFailReasons.ColorMismatch));
            Assert.That(sameColor.IsSuccess, Is.True);
        }

        [Test]
        public void DropOverTowerRule_AcceptsSlotRightAboveTopCube()
        {
            Tower tower = TowerFactory.Create();
            TowerFactory.PlaceOnTop(tower);
            TowerCube top = tower.Cubes[0];

            PlacementResult result = new DropOverTowerRule().Check(tower, TowerFactory.DropAt(top.X + 0.4f, tower.TopEdge + 0.9f));

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void InvalidRequest_IsRejected()
        {
            Tower tower = TowerFactory.Create();

            PlacementResult result = tower.TryPlace(new PlacementRequest("red", float.NaN, 1f, 5f, 5f));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.FailReason, Is.EqualTo(PlacementFailReasons.InvalidRequest));
        }
    }
}
