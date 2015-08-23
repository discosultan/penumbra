using System.Collections.Generic;

namespace Penumbra.Utilities
{
    internal class GraphNode<T>
    {
        public int Index;
        public bool IsVisited;
        public T Item;
    }

    internal class Graph<T>
    {
        private readonly List<GraphNode<T>> _nodes = new List<GraphNode<T>>();
        private readonly List<int> _startIndices = new List<int>();
        private readonly List<int> _endIndices = new List<int>();
        private int _level;

        public void Initialize(IList<T> items, int startIndex)
        {
            EnsureNodesCapacity(items);
            SetNodeValues(items);
            _startIndices.Clear();
            _endIndices.Clear();
            _startIndices.Add(startIndex);
            int endIndex = startIndex - 1;
            if (endIndex < 0)
                endIndex = items.Count - 1;
            _endIndices.Add(endIndex);
            _level = 0;
        }

        public bool HasUnvisitedSegments => _level < _startIndices.Count;

        public List<List<GraphNode<T>>> GetSegments()
        {
            var result = new List<List<GraphNode<T>>>();
            for (int i = _level; i < _startIndices.Count; i++)
            {
                var segment = new List<GraphNode<T>>();
                foreach (int j in _nodes.GetIndicesBetween(_startIndices[i], _endIndices[i]))
                    segment.Add(_nodes[j]);
                result.Add(segment);
                _level++;
            }
            return result;
        }

        public void ResolveSegments()
        {
            int first = 0;
            int indexer;
            if (_nodes[0].IsVisited)
            {
                indexer = 0;
                while (true)
                {
                    indexer = _nodes.NextIndex(indexer);
                    if (!_nodes[indexer].IsVisited)
                    {
                        first = indexer;
                        break;
                    }                    
                }
            }
            else
            {
                indexer = 0;
                while (true)
                {
                    indexer = _nodes.PreviousIndex(indexer);
                    if (!_nodes[indexer].IsVisited)
                    {
                        first = indexer;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            bool isOpen = true;
            int last = first;
            indexer = first;
            for (int i = 1; i < _nodes.Count; i++)
            {
                indexer = _nodes.NextIndex(indexer);
                var node = _nodes[indexer];
                if (isOpen)
                {                    
                    if (node.IsVisited)
                    {
                        _startIndices.Add(first);
                        _endIndices.Add(last);
                            isOpen = false;
                    }
                    else
                    {
                        last = indexer;
                    }
                }
                else
                {
                    if (!node.IsVisited)
                    {
                        isOpen = true;
                        first = indexer;
                        last = first;
                    }
                }
            }
        }

        private void EnsureNodesCapacity(IList<T> items)
        {
            while (_nodes.Count < items.Count)
                _nodes.Add(new GraphNode<T>());
        }

        private void SetNodeValues(IList<T> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                _nodes[i].IsVisited = false;
                _nodes[i].Item = items[i];
                _nodes[i].Index = i;
            }
        }
    }
}
