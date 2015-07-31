using System.Linq;
using NUnit.Framework;
using Penumbra.Utilities;

namespace Penumbra.Tests
{
    [TestFixture]
    internal class DynamicArrayTests
    {
        [Test]
        public void Add_ItemAdded()
        {
            var subject = new DynamicArray<int>();

            subject.Add(5);

            Assert.AreEqual(5, subject[0]);
            Assert.AreEqual(1, subject.Count);
        }

        [Test]
        public void Add_CapacityIncreased()
        {
            var subject = new DynamicArray<int>(1);

            subject.Add(5);
            int capacity = subject.Capacity;
            subject.Add(6);

            Assert.AreEqual(1, capacity);
            Assert.AreEqual(2, subject.Capacity);
            Assert.AreEqual(5, subject[0]);
            Assert.AreEqual(6, subject[1]);
        }

        [Test]
        public void CapacityZero_Add_DoesNotThrow()
        {
            var subject = new DynamicArray<int>(0);

            subject.Add(5);

            Assert.Pass();
        }

        [Test]
        public void AddRange_ItemsAdded()
        {
            var subject = new DynamicArray<int>();
            var range = Enumerable.Range(1, 10).ToList();

            subject.AddRange(range);

            Assert.AreEqual(10, subject.Count);
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(range[i], subject[i]);
        }

        [Test]
        public void Clear_ItemsCleared()
        {
            var subject = new DynamicArray<int>();
            var range = Enumerable.Range(1, 10).ToList();

            subject.AddRange(range);
            subject.Clear();

            Assert.AreEqual(0, subject.Count);
        }
    }
}
