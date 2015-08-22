using Microsoft.Xna.Framework;
using NUnit.Framework;
using Penumbra.Mathematics;

namespace Penumbra.Tests
{
    [TestFixture]
    public class VectorUtilTests
    {
        [Test]
        [TestCase(0.5f, 0.5f, 0, 1, Result=true)]
        [TestCase(0f, 1f, 0.5f, 0.5f, Result = false)]
        public bool IsVectorARightFromB(float aX, float aY, float bX, float bY)
        {
            var a = new Vector2(aX, aY);
            var b = new Vector2(bX, bY);

            return VectorUtil.IsADirectingRightFromB(ref a, ref b);
        }
    }
}
