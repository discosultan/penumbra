using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class AntumbraBuilder
    {
        private readonly FastList<VertexPosition2Texture> _vertices = new FastList<VertexPosition2Texture>();

        public void PreProcess()
        {
            _vertices.Clear();
        }

        public void ProcessHull(Light light, Hull hull, ref HullContext hullCtx)
        {
            if (hullCtx.UmbraIntersectionType == IntersectionType.IntersectsInsideLight)
            {
                //_vertices.Add(new VertexPosition2Texture(hullCtx.UmbraIntersectionPoint, new Vector2(0, 1)));
                _vertices.Add(hullCtx.UmbraIntersectionVertex);
                _vertices.Add(hullCtx.UmbraRightProjectedVertex);
                _vertices.Add(hullCtx.UmbraLeftProjectedVertex);
            }            
        }

        public void Build(Light light, LightVaos vaos)
        {
            if (_vertices.Count > 0)
            {
                vaos.HasAntumbra = true;                
                vaos.AntumbraVao.SetVertices(_vertices);                
            }
            else
            {
                vaos.HasAntumbra = false;
            }
            vaos.HasAntumbra = false;
        }
    }
}
