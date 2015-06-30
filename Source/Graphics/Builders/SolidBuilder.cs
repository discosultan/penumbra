using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class SolidBuilder
    {
        private readonly ArrayPool<Vector2> _vertexArrayPool;
        private readonly ArrayPool<int> _indexArrayPool;

        private readonly List<Vector2> _vertices = new List<Vector2>();
        private readonly List<int> _indices = new List<int>();
        private int _indexOffset;

        public SolidBuilder(ArrayPool<Vector2> vertexArrayPool, ArrayPool<int> indexArrayPool)
        {
            _vertexArrayPool = vertexArrayPool;
            _indexArrayPool = indexArrayPool;
        }

        public void PreProcess()
        {
            _vertices.Clear();
            _indices.Clear();
            _indexOffset = 0;
        }

        public void ProcessHull(Light light, CPUHullPart hull)
        {            
            _vertices.AddRange(hull.Inner.TransformedHullVertices);
            int offset = _indexOffset;
            _indices.AddRange(hull.Indices.Select(index => index + offset));
            _indexOffset += hull.Inner.TransformedHullVertices.Length;            
        }

        public void Build(Light light, LightVaos vaos)
        {
            if (_vertices.Count > 0)
            {
                vaos.HasSolid = true;
                Vector2[] solidVertices = _vertices.ToArrayFromPool(_vertexArrayPool);
                int[] solidIndices = _indices.ToArrayFromPool(_indexArrayPool);
                vaos.SolidVao.SetVertices(solidVertices);
                vaos.SolidVao.SetIndices(solidIndices);
                _vertexArrayPool.Release(solidVertices);
                _indexArrayPool.Release(solidIndices);
            } 
            else
            {
                vaos.HasSolid = false;
            }
        }
    }
}
