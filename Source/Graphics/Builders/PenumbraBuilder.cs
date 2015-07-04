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
            _fins.Clear();
        }

        private bool _addNext;
        private PointProcessingContext _previousCtx;
        private PointProcessingContext _firstCtx;
        private bool _addLast;
        public void ProcessHullPoint(Light light, HullPart hull, ref PointProcessingContext context)
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

        public void ProcessHull(Light light, HullPart hull)
        {            
            // 1. FIND WHICH PAIRS REQUIRE SPECIAL PROCESSING AND MARK FINS ACCORDINGLY.

            // 2. ADD NON INTERSECTING GEOMETRY.
            foreach (PenumbraFin fin in _fins.Where(x => !x.RequiresSpecialProcessing))
            {
                _vertices.Add(fin.Vertex1);
                _vertices.Add(fin.Vertex2);
                _vertices.Add(fin.Vertex3);
                _indices.Add(_indexOffset);
                _indices.Add(_indexOffset + 1);
                _indices.Add(_indexOffset + 2);
                _finPool.Release(fin);
                _indexOffset += 3;
            }

            _addNext = false;
            _addLast = false;
            _fins.Clear();
        }

        private void GetAdditionalVertices(Light light, 
            VertexPosition2Texture root, VertexPosition2Texture inner, VertexPosition2Texture outer, Vector2 intersectionPos,
            out VertexPosition2Texture newCloserVertex, out VertexPosition2Texture newFurtherVertex)
        {
            float leftLerpAmount =
                Vector2.DistanceSquared(root.Position, intersectionPos) /
                Vector2.DistanceSquared(root.Position, inner.Position);

            var newTexCoord = Vector2.Lerp(root.TexCoord, inner.TexCoord, leftLerpAmount);
            newCloserVertex = new VertexPosition2Texture(intersectionPos, newTexCoord);

            Vector2 dir = Vector2.Normalize(intersectionPos - light.Position);
            float range = light.Range / Vector2.Dot(Vector2.Normalize(root.Position - light.Position), dir);
            Vector2 newPosition2 = light.Position + dir * range; // light.Range;
            float lerpAmount2 =
                Vector2.DistanceSquared(newPosition2, inner.Position) /
                Vector2.DistanceSquared(inner.Position, outer.Position);
            var newTexCoord2 = Vector2.Lerp(inner.TexCoord, outer.TexCoord, lerpAmount2);
            newFurtherVertex = new VertexPosition2Texture(newPosition2, newTexCoord2);
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
        
        private PenumbraFin CreateFin(Light light, ref PointProcessingContext context, HullPart hull, Side side, bool testIntersection = false)
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

                        // INTERPOLATE
                    }
                }
            }

            return result;
        }        

        private void PopulateMainVertices(PenumbraFin result, Light light, ref PointProcessingContext context, ref PenumbraFinContext finContext)
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
        }

        private bool TestIntersection(PenumbraFin result, HullPart hull, ref PointProcessingContext context, ref PenumbraFinContext finContext)
        {
            Vector2[] positions = hull.TransformedHullVertices;

            Vector2 next = positions.GetNextFrom(context.Index);
            Vector2 currentToNextDir = Vector2.Normalize(next - context.Position);
            Vector2 previous = positions.GetPreviousFrom(context.Index);
            Vector2 currentToPreviousDir = Vector2.Normalize(previous - context.Position);

            Vector2 currentToInnerDir = result.Side == Side.Left ? currentToNextDir : currentToPreviousDir;
            return VectorUtil.Intersects(context.LightToPointDir, finContext.LightRightSideToCurrentDir,currentToInnerDir);
        }

        private void ClipHullFromFin(PenumbraFin result, HullPart hull, ref PointProcessingContext context, ref PenumbraFinContext finContext)
        {
            List<Vector2> sln;
            Clipper.Clip(result.MainVertices, hull.TransformedHullVertices.ToList(), out sln);
            foreach (Vector2 point in sln)
            {
                
            }  
        }




        private class PenumbraFin
        {
            public List<VertexPosition2Texture> FinalVertices = new List<VertexPosition2Texture>();

            public VertexPosition2Texture Vertex1; // hull point
            public VertexPosition2Texture Vertex2; // projected left or right
            public VertexPosition2Texture Vertex3; // projected left or right
            public Side Side;
            public bool Intersects;
            public bool RequiresSpecialProcessing;

            public List<Vector2> MainVertices
                => new List<Vector2> {Vertex1.Position, OuterProjectedVertex.Position, InnerProjectedVertex.Position };
 
            public void Reset()
            {
                Intersects = false;
                RequiresSpecialProcessing = false;
                FinalVertices.Clear();
            }

            public VertexPosition2Texture InnerProjectedVertex => Side == Side.Left ? Vertex3 : Vertex2;

            public VertexPosition2Texture OuterProjectedVertex => Side == Side.Left ? Vertex2 : Vertex3;
        }

        private struct PenumbraFinContext
        {
            public Vector2 LightLeftSideToCurrentDir;
            public Vector2 LightRightSideToCurrentDir;
        }
    }    
}