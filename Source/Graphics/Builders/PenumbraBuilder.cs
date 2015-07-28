using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class PenumbraBuilder
    {
        private const float DegreesToRotatePenumbraTowardUmbra = 0.1f;

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
                //_fins.Add(CreateFin(light, ref context, hull, Side.Left, false));
                _fins.Add(CreateFin(light, ref context, hull, Side.Right, false));
                _addNext = false;
            }
            else if (_addLast && context.Index == hull.TransformedHullVertices.Count - 1)
            {
                //_fins.Add(CreateFin(light, ref context, hull, Side.Right, false));
                _fins.Add(CreateFin(light, ref context, hull, Side.Left, false));
                _addLast = false;
            }
            else if ((context.Dot1 >= 0 && context.Dot2 < 0 ||
                context.Dot1 < 0 && context.Dot2 >= 0))
            {
                // Create penumbra fin.                        
                //Side side = context.Dot2 >= context.Dot1 ? Side.Left : Side.Right;
                Side side = context.Dot2 >= context.Dot1 ? Side.Right : Side.Left;
                PenumbraFin fin = CreateFin(light, ref context,
                    hull, side, testIntersection: true);
                _fins.Add(fin);
                if (fin.Intersects)
                {
                    switch (fin.Side)
                    {
                        case Side.Right:                        
                            if (context.Index == hull.TransformedHullVertices.Count - 1)
                            {                                
                                _fins.Add(CreateFin(light, ref _firstCtx, hull, Side.Right, false));
                            }
                            else
                            {
                                _addNext = true;
                            }                            
                            break;
                        case Side.Left:                        
                            if (context.Index == 0)
                            {
                                _addLast = true;
                            }
                            else
                            {                                
                                _fins.Add(CreateFin(light, ref _previousCtx, hull, Side.Left, false));
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
                    //if (fin.IsCreatedByIntersection) continue;

                    // CLIP FROM MID                    
                    ClipMid(fin, light, ref hullCtx);                    
                }

                // ADD TEXCOORDS, INTERPOLATE
                AddTexCoords(fin);

                if (hullCtx.UmbraIntersectionType == IntersectionType.IntersectsInsideLight)
                {
                    AttachInterpolatedVerticesToContext(fin, ref hullCtx);                    
                }

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

        private void AttachInterpolatedVerticesToContext(PenumbraFin fin, ref HullContext hullCtx)
        {
            foreach (var vertex in fin.FinalVertices)
            {
                // We can populate only 1 vertex from a single fin.
                if (Calc.NearEqual(vertex.Position, hullCtx.UmbraLeftProjectedPoint))
                {
                    hullCtx.UmbraLeftProjectedVertex = vertex;
                    return;
                }
                if (Calc.NearEqual(vertex.Position, hullCtx.UmbraRightProjectedPoint))
                {
                    hullCtx.UmbraRightProjectedVertex = vertex;
                    return;
                }
            }
        }

        public void Build(Light light, LightVaos vaos)
        {
            if (_vertices.Count > 0 && _indices.Count > 0)
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

            if (!testIntersection)
                result.IsCreatedByIntersection = true;

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
                        // REORDER VERTICES FIN ORIGIN FIRST
                        OrderFinVerticesOriginFirst(result);
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
            // ROTATE A LITTLE BIT TOWARD UMBRA TO REMOVE 1 PX INACCURACIES/FLICKERINGS.
            finContext.LightRightSideToCurrentDir = VectorUtil.Rotate(finContext.LightRightSideToCurrentDir, -MathHelper.ToRadians(DegreesToRotatePenumbraTowardUmbra));
            finContext.LightLeftSideToCurrentDir = VectorUtil.Rotate(finContext.LightLeftSideToCurrentDir, MathHelper.ToRadians(DegreesToRotatePenumbraTowardUmbra));
            // CALCULATE RANGE.
            float range = light.Range / Vector2.Dot(context.LightToPointDir, finContext.LightRightSideToCurrentDir);

            //int outerTexCoord = context.IsInAnotherHull ? 1 : 0;
            int outerTexCoord = 0;

            result.Vertex1 = new VertexPosition2Texture(context.Position, new Vector2(0, 1));
            result.Vertex3 = new VertexPosition2Texture(
                lightSide1 + finContext.LightRightSideToCurrentDir * range,
                new Vector2(result.Side == Side.Left ? outerTexCoord : 1, 0));
            result.Vertex2 = new VertexPosition2Texture(
                lightSide2 + finContext.LightLeftSideToCurrentDir * range,
                new Vector2(result.Side == Side.Left ? 1 : outerTexCoord, 0));

            result.Vertices.Add(result.Vertex1.Position);
            result.Vertices.Add(result.Vertex2.Position);
            result.Vertices.Add(result.Vertex3.Position);
        }

        private bool TestIntersection(PenumbraFin result, HullPart hull, ref HullPointContext context, ref PenumbraFinContext finContext)
        {
            var positions = hull.TransformedHullVertices;

            Vector2 next = positions.NextElement(context.Index);
            Vector2 currentToNextDir = Vector2.Normalize(next - context.Position);
            Vector2 previous = positions.PreviousElement(context.Index);
            Vector2 currentToPreviousDir = Vector2.Normalize(previous - context.Position);
            
            Vector2 currentToInnerDir = result.Side == Side.Right ? currentToNextDir : currentToPreviousDir;
            return VectorUtil.Intersects(
                context.LightToPointDir, 
                finContext.LightRightSideToCurrentDir, 
                currentToInnerDir);
        }

        private void ClipHullFromFin(PenumbraFin result, HullPart hull, ref HullPointContext context, ref PenumbraFinContext finContext)
        {
            Polygon sln;
            Polygon.Clip(result.Vertices, hull.TransformedHullVertices, out sln);
            result.Vertices = sln;
        }

        private void OrderFinVerticesOriginFirst(PenumbraFin fin)
        {
            int index = 0;
            for (int i = 0; i < fin.Vertices.Count; i++)
            {
                Vector2 pos = fin.Vertices[i];
                if (Calc.NearEqual(pos, fin.Vertex1.Position))
                {
                    index = i;
                    break;
                }
            }

            if (index != 0)
            {
                int  vertexCount = fin.Vertices.Count;
                int numToShift = vertexCount - index;                
                fin.Vertices.ShiftRight(numToShift);                
             }
        }

        private void AddTexCoords(PenumbraFin result)
        {
            foreach (Vector2 p in result.Vertices)
            {
                if (Calc.NearEqual(p, result.Vertex1.Position))
                {
                    result.FinalVertices.Add(result.Vertex1);
                }
                else if (Calc.NearEqual(p, result.Vertex2.Position))
                {
                    result.FinalVertices.Add(result.Vertex2);
                }
                else if (Calc.NearEqual(p, result.Vertex3.Position))
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
            result.Vertices.GetIndices(WindingOrder.Clockwise, result.Indices);            
        }

        private void ClipMid(PenumbraFin fin, Light light, ref HullContext hullCtx)
        {
            if (fin.Side == Side.Left)
            {
                fin.Vertices.Insert(fin.Vertices.Count - 2, hullCtx.UmbraIntersectionPoint);
                fin.Vertices[fin.Vertices.Count - 2] = hullCtx.UmbraRightProjectedPoint;
            }
            else
            {
                fin.Vertices.Insert(2, hullCtx.UmbraLeftProjectedPoint);
                fin.Vertices[3] = hullCtx.UmbraIntersectionPoint;
            }

            // --------------------------------------
                
            //Vector2 dirFromLight = hullCtx.UmbraIntersectionPoint - light.Position;
            //dirFromLight.Normalize();

            //float range = 16000; // TODO: replace with something meaningful

            //Vector2 outerPoint = light.Position + dirFromLight * range;
            //Vector2 lightPos = light.Position;
            //Vector2 intersectionPos;
            //VectorUtil.LineIntersect(ref fin.Vertex2.Position, ref fin.Vertex3.Position, ref lightPos,
            //    ref outerPoint, out intersectionPos);
            
            //if (fin.Side == Side.Left)
            //{
            //    //______
            //    //\  | /
            //    // \ |/
            //    //  \/
            //    fin.Vertices.Insert(fin.Vertices.Count - 2, hullCtx.UmbraIntersectionPoint);
            //    fin.Vertices[fin.Vertices.Count - 2] = intersectionPos;
            //}
            //else
            //{
            //    //______
            //    //\ |  /
            //    // \| /
            //    //  \/
            //    fin.Vertices.Insert(2, intersectionPos);
            //    fin.Vertices[3] = hullCtx.UmbraIntersectionPoint;
            //}

            // ---------------------------------------

            //List<Vector2> clip = new List<Vector2>();
            //clip.Add(light.Position);
            //clip.Add(hullCtx.UmbraIntersectionPoint);
            //clip.Add(outerPoint);
            //Vector2 toRightDir = VectorUtil.Rotate90CW(dirFromLight);
            //if (fin.Side == Side.Left)
            //{
            //    Vector2 rightPoint = outerPoint + toRightDir * range; // ??                
            //    clip.Add(rightPoint);
            //}
            //else
            //{
            //    Vector2 leftPoint = outerPoint - toRightDir * range; // ??
            //    clip.Add(leftPoint);
            //}

            //List<Vector2> sln;
            //Clipper.Clip(fin.Vertices, clip, out sln);
            //fin.Vertices = sln;
        }

        private class PenumbraFin
        {
            public readonly List<VertexPosition2Texture> FinalVertices = new List<VertexPosition2Texture>();
            public Polygon Vertices = new Polygon(WindingOrder.CounterClockwise); // piu piu piu TODO: readonly
            public readonly List<int> Indices = new List<int>();

            public VertexPosition2Texture Vertex1; // hull point
            public VertexPosition2Texture Vertex2; // projected left or right
            public VertexPosition2Texture Vertex3; // projected left or right            
            public Side Side;
            public bool Intersects;
            public bool IsCreatedByIntersection;         
 
            public void Reset()
            {
                Intersects = false;
                IsCreatedByIntersection = false;
                FinalVertices.Clear();
                Vertices.Clear();
                Indices.Clear();
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