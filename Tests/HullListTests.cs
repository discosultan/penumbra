using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace Penumbra.Tests
{
    [TestFixture]
    class HullListTests
    {
        [Test]
        public void HullsNotIntersecting_NotMerged()
        {
            var hull1 = NewSquare();
            hull1.Position = new Vector2(-10, 0);
            var hull2 = NewSquare();
            hull2.Position = new Vector2(10, 0);
            var list = new HullList(new ObservableCollection<Hull> {hull1, hull2});

            list.Resolve();

            Assert.AreEqual(hull1, list.ResolvedHulls[0]);
            Assert.AreEqual(hull2, list.ResolvedHulls[1]);
            Assert.AreEqual(2, list.ResolvedHulls.Count);
        }

        [Test]
        public void TwoHullsIntersect_Merged()
        {
            var hull1 = NewSquare();
            hull1.Position = new Vector2(-0.1f, 0);
            var hull2 = NewSquare();
            hull2.Position = new Vector2(0.1f, 0);
            var list = new HullList(new ObservableCollection<Hull> { hull1, hull2 });

            list.Resolve();

            Assert.AreEqual(1, list.ResolvedHulls.Count);
            Assert.AreNotEqual(hull1, list.ResolvedHulls[0]);
        }

        [Test]
        public void TwoHullsCornersIntersect_Merged()
        {
            var hull1 = NewSquare();
            var hull2 = NewSquare();
            hull1.Position = new Vector2(-0.5f, 0.5f);
            hull2.Position = new Vector2(0.5f, -0.5f);
            var list = new HullList(new ObservableCollection<Hull> { hull1, hull2 });

            list.Resolve();

            Assert.AreEqual(1, list.ResolvedHulls.Count);
            Assert.AreNotEqual(hull1, list.ResolvedHulls[0]);
        }

        private static Hull NewSquare()
        {
            return new Hull(new[]
            {
                new Vector2(0.5f, 0.5f),
                new Vector2(-0.5f, 0.5f),
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, -0.5f),
            });
        }
    }
}
