using CubeTower.Application.Config;
using CubeTower.Presentation.Input;
using NUnit.Framework;
using UnityEngine;

namespace CubeTower.Tests.EditMode
{
    public sealed class ConfigAndInputTests
    {
        [Test]
        public void Validator_ReportsBrokenConfig()
        {
            GameConfig config = new(0, new[]
            {
                new CubeColorDefinition("a", Color.red),
                new CubeColorDefinition("a", Color.red),
                new CubeColorDefinition(" ", Color.blue),
            });

            ConfigValidationReport report = GameConfigValidator.Validate(config);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.Errors, Has.Some.Contains("positive"));
            Assert.That(report.Errors, Has.Some.Contains("more than once"));
            Assert.That(report.Errors, Has.Some.Contains("empty id"));
            Assert.That(report.Warnings, Has.Some.Contains("different colors"));
        }

        [Test]
        public void Validator_WarnsAboutCountMismatch_AndPaletteUsesTheSmallerValue()
        {
            GameConfig config = new(20, new[] { new CubeColorDefinition("a", Color.red) });

            ConfigValidationReport report = GameConfigValidator.Validate(config);

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.Warnings, Is.Not.Empty);
            Assert.That(config.PaletteSize, Is.EqualTo(1));
        }

        [TestCase(10f, 1f, ExpectedResult = DragIntent.Scroll)]
        [TestCase(-10f, 3f, ExpectedResult = DragIntent.Scroll)]
        [TestCase(1f, 10f, ExpectedResult = DragIntent.PickUp)]
        [TestCase(-3f, -10f, ExpectedResult = DragIntent.PickUp)]
        [TestCase(0f, 0f, ExpectedResult = DragIntent.Undetermined)]
        public DragIntent GestureClassifier_SplitsScrollAndPickUp(float x, float y) => 
            new GestureClassifier(45f).Classify(new Vector2(x, y));
    }
}
