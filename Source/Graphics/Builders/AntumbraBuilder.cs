using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class AntumbraBuilder
    {
        private readonly FastList<VertexPosition2Texture> _vertices = new FastList<VertexPosition2Texture>();

        public void PreProcess()
        {
            _vertices.Clear(true);
        }

        public void ProcessHull(Light light, HullContext hullCtx)
        {
            //if (hullCtx.UmbraIntersectionType == IntersectionType.IntersectsInsideLight)
            //{
            //    //_vertices.Add(new VertexPosition2Texture(hullCtx.UmbraIntersectionPoint, new Vector2(0, 1)));
            //    _vertices.Add(hullCtx.UmbraIntersectionVertex);
            //    _vertices.Add(hullCtx.UmbraRightProjectedVertex);
            //    _vertices.Add(hullCtx.UmbraLeftProjectedVertex);
            //}            
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

        /* AlphaBlendFunction = BlendFunction.Subtract,
           AlphaSourceBlend = Blend.DestinationAlpha,
           AlphaDestinationBlend = Blend.InverseSourceAlpha,*/ 

        private static float BlendAlpha(float alpha)
        {
            // srcA * destA - destA * (1 - srcA)
            // destA = 1
            
            //return alpha - (1 - alpha);
            return 2f * alpha - 1f;
        }

        private static float GetAlphaForTexCoord(ref Vector2 texCoord)
        {
            return 1f - Calc.Pow(texCoord.X / (1 - texCoord.Y), 4);
        }
    }
}
