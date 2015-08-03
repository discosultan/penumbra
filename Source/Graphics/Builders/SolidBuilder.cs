using Microsoft.Xna.Framework;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class SolidBuilder
    {        
        private readonly FastList<Vector2> _vertices = new FastList<Vector2>();
        private readonly FastList<int> _indices = new FastList<int>();
        private int _indexOffset;        

        public void PreProcess()
        {
            _vertices.Clear();
            _indices.Clear();
            _indexOffset = 0;
        }

        public void ProcessHull(Light light, Hull hull)
        {
            int existingIndexCount = _indices.Count;
            _vertices.AddRange(hull.TransformedPoints);            
            _indices.AddRange(hull.Indices);
            int numVertices = hull.TransformedPoints.Count;
            for (int i = existingIndexCount; i < _indices.Count; i++)
            {
                _indices[i] = _indices[i] + _indexOffset;
            }
            _indexOffset += numVertices;
        }

        public void Build(Light light, LightVaos vaos)
        {
            if (_vertices.Count > 0)
            {
                vaos.HasSolid = true;                
                vaos.SolidVao.SetVertices(_vertices);
                vaos.SolidVao.SetIndices(_indices);                
            } 
            else
            {
                vaos.HasSolid = false;
            }
        }
    }
}
