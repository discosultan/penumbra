using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Triangulation;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class CPUPenumbraBuilder
    {
        private readonly List<VertexPosition2Texture> _vertices = new List<VertexPosition2Texture>();
        private readonly List<int> _indices = new List<int>();
        private readonly ArrayPool<VertexPosition2Texture> _vertexArrayPool = new ArrayPool<VertexPosition2Texture>();
        private readonly ArrayPool<int> _indexArrayPool;
        private readonly Pool<PenumbraFin> _finPool = new Pool<PenumbraFin>();
        private readonly List<PenumbraFin> _fins = new List<PenumbraFin>();

        private int _indexOffset;

        public CPUPenumbraBuilder(ArrayPool<int> indexArrayPool)
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
        private bool _addLast;
        public void ProcessHullPoint(Light light, CPUHullPart hull, ref PointProcessingContext context)
        {
            if (_addNext)
            {                
                _fins.Add(CreateFin(light, context, hull.Inner.TransformedHullVertices, Side.Left, false));
                _addNext = false;
            }
            else if (_addLast && context.Index == hull.Inner.TransformedHullVertices.Length - 1)
            {
                CreateFin(light, context, hull.Inner.TransformedHullVertices, Side.Right, false);
                _addLast = false;
            }
            else if ((context.Dot1 >= 0 && context.Dot2 < 0 ||
                context.Dot1 < 0 && context.Dot2 >= 0))
            {
                // Create penumbra fin.                        
                Side side = context.Dot2 >= context.Dot1 ? Side.Left : Side.Right;
                PenumbraFin fin = CreateFin(light, context,
                    hull.Inner.TransformedHullVertices, side, testIntersection: true);
                _fins.Add(fin);
                if (fin.Intersects)
                {
                    switch (fin.Side)
                    {
                        case Side.Left:
                            _addNext = true;
                            break;
                        case Side.Right:
                            if (context.Index == 0)
                            {
                                _addLast = true;
                            }
                            else
                            {
                                _fins.Add(CreateFin(light, _previousCtx, hull.Inner.TransformedHullVertices, Side.Right, false));
                            }
                            break;
                    }
                }
            }
            _previousCtx = context;
        }

        public void ProcessHull(Light light, CPUHullPart hull)
        {            
            // 1. FIND WHICH PAIRS REQUIRE SPECIAL PROCESSING AND MARK FINS ACCORDINGLY.
            var intersectingPairs = new List<IntersectionResult>();            
            //for (int i = 0; i < _fins.Count; i++)
            //{                
            //    PenumbraFin first = _fins[i];
            //    Side firstSide = first.Side;                
            //    for (int j = 1; j < _fins.Count; j++)
            //    {                    
            //        PenumbraFin second = _fins[j];
            //        if (second.Side == firstSide) continue;

            //        VertexPosition2Texture firstInner = first.InnerProjectedVertex;
            //        VertexPosition2Texture secondInner = second.InnerProjectedVertex;
            //        Vector2 intersectionPos;
            //        if (VectorUtil.LineIntersect(
            //            ref first.Vertex1.Position,
            //            ref firstInner.Position,
            //            ref second.Vertex1.Position,
            //            ref secondInner.Position,
            //            out intersectionPos))
            //        {
            //            first.RequiresSpecialProcessing = true;
            //            second.RequiresSpecialProcessing = true;
            //            intersectingPairs.Add(new IntersectionResult
            //            {
            //                First = first,
            //                Second = second,
            //                IntersectionPoint = intersectionPos
            //            });
            //        }
            //        else if (VectorUtil.PointIsInside(second.Points, first.Vertex1.Position))
            //        {
            //            first.RequiresSpecialProcessing = true;
            //            second.RequiresSpecialProcessing = true;
            //            intersectingPairs.Add(new IntersectionResult
            //            {
            //                First = first,
            //                Second = second,
            //                IntersectionPoint = first.Vertex1.Position
            //            });
            //        }
            //        else if (VectorUtil.PointIsInside(first.Points, second.Vertex1.Position))
            //        {
            //            first.RequiresSpecialProcessing = true;
            //            second.RequiresSpecialProcessing = true;
            //            intersectingPairs.Add(new IntersectionResult
            //            {
            //                First = first,
            //                Second = second,
            //                IntersectionPoint = second.Vertex1.Position
            //            });
            //        }

            //    }
            //}

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

            // 3. ADD INTERSECTING GEOMETRY AFTER SPECIAL PROCESSING.
            foreach (IntersectionResult intersectingPair in intersectingPairs)
            {
                foreach (PenumbraFin fin in intersectingPair.Fins)
                {
                    var vertices = new List<VertexPosition2Texture>();
                    VertexPosition2Texture closer, further;
                    GetAdditionalVertices(light, fin.Vertex1, fin.InnerProjectedVertex, fin.OuterProjectedVertex,
                        intersectingPair.IntersectionPoint, out closer, out further);
                    switch (fin.Side)
                    {
                        case Side.Left:
                            vertices.Add(fin.Vertex1);
                            vertices.Add(fin.Vertex2);
                            vertices.Add(further);
                            vertices.Add(closer);
                            break;
                        case Side.Right:
                            vertices.Add(fin.Vertex1);
                            vertices.Add(closer);
                            vertices.Add(further);
                            vertices.Add(fin.Vertex3);
                            break;
                    }
                    Vector2[] outputVertices;
                    int[] indices;
                    Triangulator.Triangulate(
                        vertices.Select(x => x.Position).ToArray(),
                        WindingOrder.Clockwise,
                        WindingOrder.Clockwise,
                        WindingOrder.Clockwise,
                        out outputVertices, out indices);
                    _vertices.AddRange(vertices);
                    _indices.AddRange(indices.Select(x => x + _indexOffset));
                    _indexOffset += vertices.Count;
                }
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

        private PenumbraFin CreateFin(Light light, PointProcessingContext context, Vector2[] positions, Side side, bool testIntersection = false)
        {
            PenumbraFin result = _finPool.Fetch();
            result.Reset();
            result.Side = side;

            Vector2 lightToCurrent90CWDir = VectorUtil.Rotate90CW(context.LightToPointDir);
            Vector2 toLightSide = lightToCurrent90CWDir * light.Radius;
            Vector2 lightSide1 = light.Position + toLightSide;
            Vector2 lightSide2 = light.Position - toLightSide;
            Vector2 lightSide1ToCurrentDir = Vector2.Normalize(context.Position - lightSide1);
            Vector2 lightSide2ToCurrentDir = Vector2.Normalize(context.Position - lightSide2);
            float range = light.Range / Vector2.Dot(context.LightToPointDir, lightSide1ToCurrentDir);

            //int outerTexCoord = context.IsInAnotherHull ? 1 : 0;
            int outerTexCoord = 0;

            result.Vertex1 = new VertexPosition2Texture(context.Position, new Vector2(0, 1));
            result.Vertex2 = new VertexPosition2Texture(
                lightSide1 + lightSide1ToCurrentDir * range,
                new Vector2(side == Side.Left ? outerTexCoord : 1, 0));
            result.Vertex3 = new VertexPosition2Texture(
                lightSide2 + lightSide2ToCurrentDir * range,
                new Vector2(side == Side.Left ? 1 : outerTexCoord, 0));

            // Check for intersection.
            if (testIntersection)
            {
                Vector2 next = positions.GetNextFrom(context.Index);
                Vector2 currentToNextDir = Vector2.Normalize(next - context.Position);
                Vector2 previous = positions.GetPreviousFrom(context.Index);
                Vector2 currentToPreviousDir = Vector2.Normalize(previous - context.Position);

                Vector2 lightSideToCurrentInnerDir;
                Vector2 lightSideToCurrentOuterDir;
                Vector2 currentToInnerDir;
                Vector2 currentToOuterDir;
                if (side == Side.Left)
                {
                    lightSideToCurrentInnerDir = lightSide2ToCurrentDir;
                    lightSideToCurrentOuterDir = lightSide1ToCurrentDir;
                    currentToInnerDir = currentToNextDir;
                    currentToOuterDir = currentToPreviousDir;
                }
                else
                {
                    lightSideToCurrentInnerDir = lightSide1ToCurrentDir;
                    lightSideToCurrentOuterDir = lightSide2ToCurrentDir;
                    currentToInnerDir = currentToPreviousDir;
                    currentToOuterDir = currentToNextDir;
                }

                //float dot = Vector2.Dot(lightToCurrentDir, lightSideToCurrentInnerDir);
                //float dotInner = Vector2.Dot(lightToCurrentDir, currentToInnerDir);
                if (VectorUtil.Intersects(context.LightToPointDir, lightSide1ToCurrentDir, currentToInnerDir)) // Intersection test with hull inner side.
                {
                    // TODO: TEMP DISABLE EXTRA SHADOW GEN
                    result.Intersects = true;

                    var from = result.InnerProjectedVertex;
                    var to = result.OuterProjectedVertex;
                    Vector2 asd = context.Position + currentToInnerDir * range;
                    Vector2 intersectionPos;
                    VectorUtil.LineIntersect(ref from.Position, ref to.Position, ref context.Position, ref asd, out intersectionPos);
                    float leftLerpAmount =
                        Vector2.DistanceSquared(from.Position, intersectionPos) /
                        Vector2.DistanceSquared(from.Position, to.Position);
                    Vector2 tex = Vector2.Lerp(from.TexCoord, to.TexCoord, leftLerpAmount);
                    if (result.Side == Side.Left)
                    {
                        result.Vertex3 = new VertexPosition2Texture(intersectionPos, tex);
                    }
                    else
                    {
                        result.Vertex2 = new VertexPosition2Texture(intersectionPos, tex);                        
                    }

                    //float dotOuter = Vector2.Dot(lightToCurrentDir, currentToOuterDir);

                    // TODO: TEMP DISABLE OUTER INTERSECTION
                    //if (VectorUtil.Intersects(context.LightToPointDir, lightSideToCurrentInnerDir, currentToOuterDir)) // Intersection test with hull outer side.
                    //{
                    //    float project = side == Side.Left ? 1f : -1f;

                    //    float angleReduced = Calc.Acos(Vector2.Dot(currentToOuterDir, lightSideToCurrentInnerDir)) * project;
                    //    lightSideToCurrentOuterDir = VectorUtil.Rotate(lightSideToCurrentOuterDir, angleReduced);

                    //    range = light.Range / Vector2.Dot(context.LightToPointDir, lightSideToCurrentOuterDir);
                    //    Vector2 positionInner = context.Position + currentToOuterDir * range; // TODO: wrong range
                    //    //Vector2 positionOuter = lightSide1 + lightSideToCurrentOuterDir * range;
                    //    // Clamp penumbra outer to hull outer.
                    //    if (side == Side.Left)
                    //    {
                    //        result.Vertex3.Position = positionInner;
                    //        //result.Vertex2.Position = positionOuter;
                    //    }
                    //    else
                    //    {
                    //        result.Vertex2.Position = positionInner;
                    //    }
                    //}
                }
            }

            return result;
        }

        private class IntersectionResult
        {
            public PenumbraFin First;
            public PenumbraFin Second;
            public Vector2 IntersectionPoint;

            public PenumbraFin[] Fins => new[]
            {
                First, Second
            };
        }

        private class PenumbraFin
        {
            public VertexPosition2Texture Vertex1; // hull point
            public VertexPosition2Texture Vertex2; // projected left or right
            public VertexPosition2Texture Vertex3; // projected left or right
            public Side Side;
            public bool Intersects;
            public bool RequiresSpecialProcessing;

            public void Reset()
            {
                Intersects = false;
                RequiresSpecialProcessing = false;
            }

            public Vector2[] Points => new[]
            {
                Vertex1.Position,
                Vertex2.Position,
                Vertex3.Position
            };

            public VertexPosition2Texture InnerProjectedVertex => Side == Side.Left ? Vertex3 : Vertex2;

            public VertexPosition2Texture OuterProjectedVertex => Side == Side.Left ? Vertex2 : Vertex3;
        }        
    }    
}