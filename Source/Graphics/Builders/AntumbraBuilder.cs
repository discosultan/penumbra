using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Penumbra.Graphics.Builders
{
    internal class AntumbraBuilder
    {
        private readonly List<VertexPosition2Texture> _vertices = new List<VertexPosition2Texture>();

        public void PreProcess()
        {
            _vertices.Clear();
        }

        public void ProcessHull(Light light, HullPart hull, ref HullContext hullCtx)
        {
            if (hullCtx.UmbraIntersectionType == IntersectionType.IntersectsInsideLight)
            {
                _vertices.Add(new VertexPosition2Texture(hullCtx.UmbraIntersectionPoint, new Vector2(0, 1)));
                _vertices.Add(hullCtx.UmbraRightProjectedVertex);
                _vertices.Add(hullCtx.UmbraLeftProjectedVertex);                
            }            
        }

        public void Build(Light light, LightVaos vaos)
        {
            if (_vertices.Count > 0)
            {
                vaos.HasAntumbra = true;
                VertexPosition2Texture[] antumbraVertices = _vertices.ToArray();                
                vaos.AntumbraVao.SetVertices(antumbraVertices);                
            }
            else
            {
                vaos.HasAntumbra = false;
            }
        }
    }
}
