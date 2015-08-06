using Microsoft.Xna.Framework;
using NUnit.Framework;
using Penumbra.Mathematics.Collision;

namespace Penumbra.Tests
{
    [TestFixture]
    internal class CollisionTests
    {
        #region Line

        [Test]
        public void LineLine_Intersect()
        {
            var l1 = new Line2D(new Vector2(-2, -2), new Vector2(-1, -1));
            var l2 = new Line2D(new Vector2(+2, -2), new Vector2(-1, +1));

            Vector2 intersectionPoint;
            bool result = l1.Intersects(ref l2, out intersectionPoint);

            Assert.IsTrue(result);
            Assert.AreEqual(Vector2.Zero, intersectionPoint);
        }

        [Test]
        public void LineLine_DoNotIntersect_Parallel()
        {
            var l1 = new Line2D(new Vector2(-1, -1), new Vector2(+1, +1));
            var l2 = new Line2D(new Vector2(-1, -2), new Vector2(+1, +0));

            Vector2 intersectionPoint;
            bool result = l1.Intersects(ref l2, out intersectionPoint);

            Assert.IsFalse(result);
        }

        [Test]
        [Ignore("Currently no two exact same line can ever intersect.")]
        public void LineLine_Intersect_Equal()
        {
            var l1 = new Line2D(new Vector2(-1, -1), new Vector2(+1, +1));
            var l2 = new Line2D(new Vector2(-1, -1), new Vector2(+1, +1));

            Vector2 intersectionPoint;
            bool result = l1.Intersects(ref l2, out intersectionPoint);

            Assert.IsTrue(result);
        }

        #endregion
        #region LineSegment

        //[Test]
        //public void LineSegmentLineSegment_Intersect()
        //{
        //    var ls1 = new LineSegment2D(new Vector2(-1, -1), new Vector2(+1, +1));
        //    var ls2 = new LineSegment2D(new Vector2(+1, -1), new Vector2(-1, +1));

        //    Vector2 intersectionPoint;
        //    bool result = ls1.Intersects(ref ls2, out intersectionPoint);

        //    Assert.IsTrue(result);
        //    Assert.AreEqual(Vector2.Zero, intersectionPoint);
        //}

        //[Test]
        //public void LineSegmentLineSegment_DoNotIntersect()
        //{
        //    var ls1 = new LineSegment2D(new Vector2(-2, -2), new Vector2(-1, -1));
        //    var ls2 = new LineSegment2D(new Vector2(+2, -2), new Vector2(-1, +1));

        //    Vector2 intersectionPoint;
        //    bool result = ls1.Intersects(ref ls2, out intersectionPoint);

        //    Assert.IsFalse(result);
        //}

        //[Test]
        //[Ignore("Currently no two exact same line segments can ever intersect.")]
        //public void LineSegmentLineSegment_Intersect_Equal()
        //{
        //    var ls1 = new LineSegment2D(new Vector2(-1, -1), new Vector2(+1, +1));
        //    var ls2 = new LineSegment2D(new Vector2(-1, -1), new Vector2(+1, +1));

        //    Vector2 intersectionPoint;
        //    bool result = ls1.Intersects(ref ls2, out intersectionPoint);

        //    Assert.IsTrue(result);
        //}

        #endregion
        #region Ray

        [Test]
        public void RayRay_Intersect()
        {
            var r1 = new Ray2D(new Vector2(-2, -2), new Vector2(+1, +1));
            var r2 = new Ray2D(new Vector2(+2, -2), new Vector2(-1, +1));

            float distance;
            bool result = r1.Intersects(ref r2, out distance);
            Vector2 intersectionPoint = r1.GetPoint(distance);

            Assert.IsTrue(result);
            Assert.AreEqual(Vector2.Zero, intersectionPoint);
        }

        [Test]
        public void RayRay_DoNotIntersect()
        {
            var r1 = new Ray2D(new Vector2(-2, -2), new Vector2(+1, +1));
            var r2 = new Ray2D(new Vector2(+2, -2), new Vector2(-1, -1));

            float distance;
            bool result = r1.Intersects(ref r2, out distance);

            Assert.IsFalse(result);            
        }

        [Test]
        [Ignore("Currently no two exact same rays can ever intersect.")]
        public void RayRay_Intersect_Equal()
        {
            var r1 = new Ray2D(new Vector2(-1, -1), new Vector2(+1, +1));
            var r2 = new Ray2D(new Vector2(-1, -1), new Vector2(+1, +1));

            float distance;
            bool result = r1.Intersects(ref r2, out distance);

            Assert.IsTrue(result);
        }

        #endregion
        #region Mixed

        [Test]
        public void RayLineSegment_Intersect()
        {
            var r = new Ray2D(new Vector2(+0, -1), new Vector2(+0, +1));
            var l = new LineSegment2D(new Vector2(-1, +0), new Vector2(+1, +0));

            float distance;            
            bool result = r.Intersects(ref l, out distance);
            Vector2 intersectionPoint = r.GetPoint(distance);

            Assert.IsTrue(result);
            Assert.AreEqual(Vector2.Zero, intersectionPoint);
        }

        [Test]
        public void RayLineSegment_DoNotIntersect()
        {
            var r = new Ray2D(new Vector2(+0, -1), new Vector2(+0, +1));
            var l = new LineSegment2D(new Vector2(-2, +0), new Vector2(-1, +0));
            
            bool result = r.Intersects(ref l);            

            Assert.IsFalse(result);            
        }

        [Test]
        public void RayLineSegment_Intersect_Side_Case1()
        {
            var r = new Ray2D(new Vector2(+0, -1), new Vector2(+0, +1));
            var ls = new LineSegment2D(new Vector2(+0, +0), new Vector2(+1, +0));

            float distance;
            bool result = r.Intersects(ref ls, out distance);
            Vector2 intersectionPoint = r.GetPoint(distance);

            Assert.IsTrue(result);
            Assert.AreEqual(Vector2.Zero, intersectionPoint);
        }

        [Test]
        public void RayLineSegment_Intersect_Side_Case2()
        {
            var r = new Ray2D(new Vector2(-1, -1), new Vector2(+0, +1));
            var ls = new LineSegment2D(new Vector2(-20, +2), new Vector2(-1, -1));

            float distance;
            bool result = r.Intersects(ref ls, out distance);
            Vector2 intersectionPoint = r.GetPoint(distance);

            Assert.IsTrue(result);
            Assert.AreEqual(new Vector2(-1, -1), intersectionPoint);
        }

        [Test]
        public void RayLineSegment_Intersect_Side_Case3()
        {
            var dir = Vector2.Normalize(new Vector2(0, 0) - new Vector2(-18, -50));
            //var dir = new Vector2(0.3713907f, 0.9284767f);
            var r = new Ray2D(new Vector2(-18, -50), dir);
            var ls = new LineSegment2D(new Vector2(+0, +50), new Vector2(+0, +0));

            float distance;
            bool result = r.Intersects(ref ls, out distance);
            Vector2 intersectionPoint = r.GetPoint(distance);

            Assert.IsTrue(result);
            Assert.AreEqual(new Vector2(0, 0), intersectionPoint);
        }

        #endregion
    }
}
