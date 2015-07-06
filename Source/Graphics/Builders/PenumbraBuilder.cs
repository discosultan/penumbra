using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Clipping;
using Penumbra.Mathematics.Triangulation;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class PenumbraBuilder
    {
        private readonly List<VertexPosition2Texture> _vertices = new List<VertexPosition2Texture>();
        private readonly List<int> _indices = new List<int>();
        private readonly ArrayPool<VertexPosition2Texture> _vertexArrayPool = new ArrayPool<VertexPosition2Texture>();
        private readonly ArrayPool<int> _indexArrayPool;
        private readonly Pool<PenumbraFin> _finPool = new Pool<PenumbraFin>();
        private readonly List<PenumbraFin> _fins = new List<PenumbraFin>();

        private int _indexOffset;

        public PenumbraBuilder(ArrayPool<int> indexArrayPool)
        {
            _indexArrayPool = indexArrayPool;
        }

        public void PreProcess()
        {
            _indexOffset = 0;
            _vertices.Clear();
            _indices.Clear();
            //_fins.Clear();
        }

        private bool _addNext;
        private HullPointContext _previousCtx;
        private HullPointContext _firstCtx;
        private bool _addLast;
        public void ProcessHullPoint(Light light, HullPart hull, ref HullPointContext context)
        {
            if (context.Index == 0)
            {
                _firstCtx = context;
            }

            if (_addNext)
            {                
                _fins.Add(CreateFin(light, ref context, hull, Side.Left, false));
                _addNext = false;
            }
            else if (_addLast && context.Index == hull.TransformedHullVertices.Length - 1)
            {
                _fins.Add(CreateFin(light, ref context, hull, Side.Right, false));
                _addLast = false;
            }
            else if ((context.Dot1 >= 0 && context.Dot2 < 0 ||
                context.Dot1 < 0 && context.Dot2 >= 0))
            {
                // Create penumbra fin.                        
                Side side = context.Dot2 >= context.Dot1 ? Side.Left : Side.Right;
                PenumbraFin fin = CreateFin(light, ref context,
                    hull, side, testIntersection: true);
                _fins.Add(fin);
                if (fin.Intersects)
                {
                    switch (fin.Side)
                    {
                        case Side.Left:
                            if (context.Index == hull.TransformedHullVertices.Length - 1)
                            {
                                _fins.Add(CreateFin(light, ref _firstCtx, hull, Side.Left, false));
                            }
                            else
                            {
                                _addNext = true;
                            }                            
                            break;
                        case Side.Right:
                            if (context.Index == 0)
                            {
                                _addLast = true;
                            }
                            else
                            {
                                _fins.Add(CreateFin(light, ref _previousCtx, hull, Side.Right, false));
                            }
                            break;
                    }
                }
            }
            _previousCtx = context;
        }

        public void ProcessHull(Light light, HullPart hull, ref HullContext hullCtx)
        {            
            foreach (PenumbraFin fin in _fins)
            {
                if (hullCtx.UmbraIntersectionType == IntersectionType.IntersectsInsideLight)
                {
                    // CLIP FROM MID
                    //context.
                    ClipMid(fin, light, ref hullCtx);
                }

                // ADD TEXCOORDS, INTERPOLATE
                AddTexCoords(fin);

                // TRIANGULATE
                TriangulateFin(fin);

                _vertices.AddRange(fin.FinalVertices);
                _indices.AddRange(fin.Indices.Select(index => index + _indexOffset));
                _finPool.Release(fin);
                _indexOffset += fin.FinalVertices.Count;
            }

            _addNext = false;
            _addLast = false;
            _fins.Clear();
        }

        private void ClipMid(PenumbraFin fin, Light light, ref HullContext hullCtx)
        {
            Vector2 dirFromLight = hullCtx.UmbraIntersectionPoint - light.Position;
            dirFromLight.Normalize();

            Vector2 outerPoint = light.Position + dirFromLight*light.Range;

            List<Vector2> clip = new List<Vector2>();
            clip.Add(light.Position);
            clip.Add(outerPoint);
            Vector2 toRightDir = VectorUtil.Rotate90CW(dirFromLight);
            if (fin.Side == Side.Left)
            {                
                Vector2 rightPoint = outerPoint + toRightDir*light.Range; // ??                
                clip.Add(rightPoint);
            }
            else
            {                
                Vector2 leftPoint = outerPoint - toRightDir * light.Range; // ??
                clip.Add(leftPoint);
            }

            List<Vector2> sln;
            Clipper.Clip(fin.Vertices, clip, out sln);
            fin.Vertices = sln;
        }

        public void Build(Light light, LightVaos vaos)
        {
            if (_vertices.Count > 0)
            {
                vaos.HasPenumbra = true;
                VertexPosition2Texture[] penumbraVertices = _vertices.ToArrayFromPool(_vertexArrayPool);
                int[] penumbraIndices = _indices.ToArrayFromPool(_indexArrayPool);
                vaos.PenumbraVao.SetVertices(penumbraVertices);
                vaos.PenumbraVao.SetIndices(penumbraIndices);
                _vertexArrayPool.Release(penumbraVertices);
                _indexArrayPool.Release(penumbraIndices);
            } 
            else
            {
                vaos.HasPenumbra = false;
            }
        }
        
        private PenumbraFin CreateFin(Light light, ref HullPointContext context, HullPart hull, Side side, bool testIntersection = false)
        {
            PenumbraFin result = _finPool.Fetch();
            result.Reset();
            result.Side = side;
            var finContext = new PenumbraFinContext();            

            // FIND MAIN VERTICES
            PopulateMainVertices(result, light, ref context, ref finContext);

            if (testIntersection)
            {
                // TEST INTERSECTION
                if (TestIntersection(result, hull, ref context, ref finContext))
                {
                    result.Intersects = true;

                    if (light.ShadowType == ShadowType.Occluded)
                    {
                        // CLIP HULL VERTICES
                        ClipHullFromFin(result, hull, ref context, ref finContext);                        
                    }
                }
            }            

            return result;
        }

        private void PopulateMainVertices(PenumbraFin result, Light light, ref HullPointContext context, ref PenumbraFinContext finContext)
        {
            Vector2 lightToCurrent90CWDir = VectorUtil.Rotate90CW(context.LightToPointDir);
            Vector2 toLightSide = lightToCurrent90CWDir * light.Radius;
            Vector2 lightSide1 = light.Position + toLightSide;
            Vector2 lightSide2 = light.Position - toLightSide;
            finContext.LightRightSideToCurrentDir = Vector2.Normalize(context.Position - lightSide1);
            finContext.LightLeftSideToCurrentDir = Vector2.Normalize(context.Position - lightSide2);
            float range = light.Range / Vector2.Dot(context.LightToPointDir, finContext.LightRightSideToCurrentDir);

            //int outerTexCoord = context.IsInAnotherHull ? 1 : 0;
            int outerTexCoord = 0;

            result.Vertex1 = new VertexPosition2Texture(context.Position, new Vector2(0, 1));
            result.Vertex2 = new VertexPosition2Texture(
                lightSide1 + finContext.LightRightSideToCurrentDir * range,
                new Vector2(result.Side == Side.Left ? outerTexCoord : 1, 0));
            result.Vertex3 = new VertexPosition2Texture(
                lightSide2 + finContext.LightLeftSideToCurrentDir * range,
                new Vector2(result.Side == Side.Left ? 1 : outerTexCoord, 0));

            result.Vertices.Add(result.Vertex1.Position);
            result.Vertices.Add(result.Vertex2.Position);
            result.Vertices.Add(result.Vertex3.Position);
        }

        private bool TestIntersection(PenumbraFin result, HullPart hull, ref HullPointContext context, ref PenumbraFinContext finContext)
        {
            Vector2[] positions = hull.TransformedHullVertices;

            Vector2 next = positions.GetNextFrom(context.Index);
            Vector2 currentToNextDir = Vector2.Normalize(next - context.Position);
            Vector2 previous = positions.GetPreviousFrom(context.Index);
            Vector2 currentToPreviousDir = Vector2.Normalize(previous - context.Position);

            Vector2 currentToInnerDir = result.Side == Side.Left ? currentToNextDir : currentToPreviousDir;
            return VectorUtil.Intersects(context.LightToPointDir, finContext.LightRightSideToCurrentDir,currentToInnerDir);
        }

        private void ClipHullFromFin(PenumbraFin result, HullPart hull, ref HullPointContext context, ref PenumbraFinContext finContext)
        {
            List<Vector2> sln;
            Clipper.Clip(result.Vertices, hull.TransformedHullVertices.ToList(), out sln);
            result.Vertices = sln;
        }

        private void AddTexCoords(PenumbraFin result)
        {
            foreach (Vector2 p in result.Vertices)
            {
                if (p == result.Vertex1.Position)
                {
                    result.FinalVertices.Add(result.Vertex1);
                }
                else if (p == result.Vertex2.Position)
                {
                    result.FinalVertices.Add(result.Vertex2);
                }
                else if (p == result.Vertex3.Position)
                {
                    result.FinalVertices.Add(result.Vertex3);
                }
                else
                {
                    result.FinalVertices.Add(new VertexPosition2Texture(
                        p,
                        InterpolateTexCoord(result, p)));
                }
            }
        }

        private Vector2 InterpolateTexCoord(PenumbraFin result, Vector2 pos)
        {
            var f = new Vector3(pos, 0);

            // ref: http://answers.unity3d.com/questions/383804/calculate-uv-coordinates-of-3d-point-on-plane-of-m.html
            var p1 = new Vector3(result.Vertex1.Position, 0);
            var p2 = new Vector3(result.Vertex2.Position, 0);
            var p3 = new Vector3(result.Vertex3.Position, 0);

            // calculate vectors from point f to vertices p1, p2 and p3:
            var f1 = p1 - f;
            var f2 = p2 - f;
            var f3 = p3 - f;
            // calculate the areas (parameters order is essential in this case):
            var va = Vector3.Cross(p1 - p2, p1 - p3); // main triangle cross product
            var va1 = Vector3.Cross(f2, f3); // p1's triangle cross product
            var va2 = Vector3.Cross(f3, f1); // p2's triangle cross product
            var va3 = Vector3.Cross(f1, f2); // p3's triangle cross product
            var a = va.Length(); // main triangle area
            // calculate barycentric coordinates with sign:
            var a1 = va1.Length() / a * Math.Sign(Vector3.Dot(va, va1));
            var a2 = va2.Length() / a * Math.Sign(Vector3.Dot(va, va2));
            var a3 = va3.Length() / a * Math.Sign(Vector3.Dot(va, va3));
            // find the uv corresponding to point f (uv1/uv2/uv3 are associated to p1/p2/p3):
            var uv = result.Vertex1.TexCoord * a1 + result.Vertex2.TexCoord * a2 + result.Vertex3.TexCoord * a3;

            return uv;
        }

        private void TriangulateFin(PenumbraFin result)
        {
            Vector2[] dummy;
            int[] indices;
            Triangulator.Triangulate(result.Vertices.ToArray(), WindingOrder.Clockwise, WindingOrder.Clockwise, WindingOrder.Clockwise, out dummy, out indices);
            result.Indices = indices.ToList();
        }

        private class PenumbraFin
        {
            public readonly List<VertexPosition2Texture> FinalVertices = new List<VertexPosition2Texture>();
            public List<Vector2> Vertices = new List<Vector2>();
            public List<int> Indices = new List<int>();

            public VertexPosition2Texture Vertex1; // hull point
            public VertexPosition2Texture Vertex2; // projected left or right
            public VertexPosition2Texture Vertex3; // projected left or right            
            public Side Side;
            public bool Intersects;            
 
            public void Reset()
            {
                Intersects = false;                
                FinalVertices.Clear();
                Vertices.Clear();
            }

            //public VertexPosition2Texture InnerProjectedVertex => Side == Side.Left ? Vertex3 : Vertex2;

            //public VertexPosition2Texture OuterProjectedVertex => Side == Side.Left ? Vertex2 : Vertex3;
        }

        private struct PenumbraFinContext
        {
            public Vector2 LightLeftSideToCurrentDir;
            public Vector2 LightRightSideToCurrentDir;
        }
    }    
}