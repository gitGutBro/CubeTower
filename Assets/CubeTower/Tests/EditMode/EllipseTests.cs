using CubeTower.Core.Geometry;
using NUnit.Framework;

namespace CubeTower.Tests.EditMode
{
    public sealed class EllipseTests
    {
        [TestCase(0f, 0f, ExpectedResult = true)]
        [TestCase(199f, 0f, ExpectedResult = true)]
        [TestCase(0f, 49f, ExpectedResult = true)]
        [TestCase(201f, 0f, ExpectedResult = false)]
        [TestCase(0f, 51f, ExpectedResult = false)]
        // Corner of the bounding rectangle: inside the rect, outside the oval.
        [TestCase(190f, 45f, ExpectedResult = false)]
        public bool Contains_RespectsOvalShape(float x, float y) => 
            Ellipse.Contains(x, y, 200f, 50f);

        [Test]
        public void Contains_IsFalseForDegenerateEllipse() => 
            Assert.That(Ellipse.Contains(0f, 0f, 0f, 10f), Is.False);
    }
}
