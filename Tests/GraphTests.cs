using System.Linq;
using NUnit.Framework;
using Penumbra.Utilities;

namespace Penumbra.Tests
{

    [TestFixture]
    public class GraphTests
    {
        [Test]
        public void GetSegments()
        {
            var graph = new Graph<int>();
            var seq = new[] {1, 2, 3};
            graph.Initialize(seq, 0);

            var result = graph.GetSegments();  
                      
            Assert.That(result.Count == 1);
            Assert.That(result[0].Count == 3);
            Assert.That(seq.SequenceEqual(result[0].Select(x => x.Item)));
        }

        [Test]
        public void ResolveSegments_OneVisited_OneSegment()
        {
            var graph = new Graph<int>();
            var seq = new[] { 1, 2, 3 };
            graph.Initialize(seq, 0);
            var nodes = graph.GetSegments();
            nodes[0][0].IsVisited = true;

            graph.ResolveSegments();
            var result = graph.GetSegments();

            Assert.That(result.Count == 1);
            Assert.That(result[0].Count == 2);
            Assert.That(!result[0].Select(x => x.Item).Contains(1));
        }

        [Test]
        public void ResolveSegments_TwoVisited_TwoSegments()
        {
            var graph = new Graph<int>();
            var seq = new[] { 1, 2, 3, 4, 5 };
            graph.Initialize(seq, 0);
            var nodes = graph.GetSegments();
            nodes[0][1].IsVisited = true;
            nodes[0][3].IsVisited = true;

            graph.ResolveSegments();
            var result = graph.GetSegments();

            Assert.That(result.Count == 2);
            Assert.That(result[0].Count == 2);
            Assert.That(result[1].Count == 1);
            Assert.That(result[0].Select(x => x.Item).SequenceEqual(new[] { 5, 1 }));
            Assert.That(result[1][0].Item == 3);
        }
    }
}
