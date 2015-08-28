using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Geometry;
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
            //_vertices.Add(new VertexPosition2Texture(hullCtx.UmbraIntersectionPoint, new Vector2(0, 1)));
            foreach (var intersectionCtx in hullCtx.UmbraIntersectionContexts)
            {
                ProcessIntersection(light, intersectionCtx);                    
            }                                  
        }

        private void ProcessIntersection(Light light, UmbraIntersectionContext intersectionCtx)
        {
            //for (int i = 0; i < intersectionCtx.UmbraLeftProjectedVertices.Count; i++)
            //{
            //    _vertices.Add(intersectionCtx.UmbraIntersectionVertices[i]);
            //    _vertices.Add(intersectionCtx.UmbraRightProjectedVertices[i]);
            //    _vertices.Add(intersectionCtx.UmbraLeftProjectedVertices[i]);

            //    //_vertices.Add(AverageVertices(intersectionCtx.UmbraIntersectionVertices));
            //    //_vertices.Add(AverageVertices(intersectionCtx.UmbraRightProjectedVertices));
            //    //_vertices.Add(AverageVertices(intersectionCtx.UmbraLeftProjectedVertices));
            //}

            var ls2 = new LineSegment2D(light.Position, light.Position + (intersectionCtx.UmbraIntersectionPoint - light.Position) * 9999);

            foreach (var left in intersectionCtx.LeftVertices)
            {
                var l = left;
                _vertices.Add(left.Item1);
                _vertices.Add(left.Item2);

                var ls = new LineSegment2D(left.Item2.Position, left.Item3.Position);
                var intersection = ls.Intersects(ls2);

                _vertices.Add(VertexPosition2Texture.InterpolateTexCoord(ref l.Item1, ref l.Item2,
                    ref l.Item3, ref intersection.IntersectionPoint));

                //foreach (var right in intersectionCtx.RightVertices)
                //{
                //    var r = right;
                //    var ls = new LineSegment2D(left.Item2.Position, right.Item2.Position);
                //    var intersection = ls.Intersects(ls2);

                //    _vertices.Add(left.Item1);
                //    _vertices.Add(left.Item2);
                //    _vertices.Add(VertexPosition2Texture.InterpolateTexCoord(ref l.Item1, ref l.Item2,
                //        ref r.Item2, ref intersection.IntersectionPoint));

                    //_vertices.Add(AverageVertices(left.Item1, right.Item1));
                    //_vertices.Add(left.Item2);
                    //_vertices.Add(right.Item2);

                    //_vertices.Add(right.Item1);                    
                    //_vertices.Add(left.Item2);
                    //_vertices.Add(right.Item2);
                //}
            }

            foreach (var left in intersectionCtx.RightVertices)
            {
                var l = left;
                _vertices.Add(left.Item1);

                var ls = new LineSegment2D(left.Item2.Position, left.Item3.Position);
                var intersection = ls.Intersects(ls2);

                _vertices.Add(VertexPosition2Texture.InterpolateTexCoord(ref l.Item1, ref l.Item2,
                    ref l.Item3, ref intersection.IntersectionPoint));

                _vertices.Add(left.Item2);                
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

        /* AlphaBlendFunction = BlendFunction.Subtract,
           AlphaSourceBlend = Blend.DestinationAlpha,
           AlphaDestinationBlend = Blend.InverseSourceAlpha,*/

        private VertexPosition2Texture AverageVertices(params VertexPosition2Texture[] vertices)
        {
            Vector2 texCoord = Vector2.Zero;
            for (int i = 0; i < vertices.Length; i++)
            {
                texCoord += vertices[i].TexCoord;
            }
            texCoord /= vertices.Length;
            return new VertexPosition2Texture(vertices[0].Position, texCoord);
        }

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
