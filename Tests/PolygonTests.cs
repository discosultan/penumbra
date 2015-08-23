using Microsoft.Xna.Framework;
using NUnit.Framework;
using Penumbra.Mathematics;
using Shouldly;

namespace Penumbra.Tests
{
    [TestFixture]
    public class PolygonTests
    {
        [Test]
        public void IsSimple_Convex_True()
        {
            var polygon = new Polygon(new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));

            polygon.IsSimple().ShouldBe(true);
        }

        [Test]
        public void IsSimple_Convex_False()
        {
            var polygon = new Polygon(new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(1, 0));

            polygon.IsSimple().ShouldBe(false);
        }

        [Test]
        public void IsSimple_ConcaveTangent_False()
        {
            var polygon = new Polygon(new Vector2(0, 0), new Vector2(1, 1), new Vector2(2, 0), new Vector2(0, 2), new Vector2(1, 1), new Vector2(0, 2));

            polygon.IsSimple().ShouldBe(false);
        }
    }
}
