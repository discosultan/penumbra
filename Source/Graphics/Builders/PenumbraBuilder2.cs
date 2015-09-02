using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Geometry;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class PenumbraBuilder2
    {
        private const float DegreesToRotatePenumbraTowardUmbra = 0.1f;

        private readonly FastList<VertexPosition2Texture> _vertices = new FastList<VertexPosition2Texture>();
        private readonly FastList<int> _indices = new FastList<int>();    
            
        private readonly Pool<PenumbraFin> _finPool = new Pool<PenumbraFin>();
        private readonly List<PenumbraFin> _fins = new List<PenumbraFin>();

        private int _indexOffset;

        public void PreProcess()
        {
            _indexOffset = 0;
            _vertices.Clear();
            _indices.Clear();
        }

        public void ProcessHull(Light light, Hull hull, HullContext hullCtx)
        {
            var points = hullCtx.PointContexts;
            for (int i = 0; i < points.Count; i++)
            {
                var ctx = points[i];
                if (ctx.IsConvex && (ctx.RightSide == Side.Right
                    || ctx.Side == Side.Right
                    ))
                {
                    _fins.Add(CreateFin(light, ref ctx, hull, Side.Right));
                }
                else if (ctx.IsConvex && (ctx.LeftSide == Side.Left
                    || ctx.Side == Side.Left
                    ))
                {
                    _fins.Add(CreateFin(light, ref ctx, hull, Side.Left));
                }
            }

            foreach (PenumbraFin fin in _fins)
            {
                int intersectionsCount = hullCtx.UmbraIntersectionContexts.Count;
                for (int i = 0; i < intersectionsCount; i++)
                {                    
                    var intersectionCtx = hullCtx.UmbraIntersectionContexts[i];
                    var segmentToTestAgainst = new LineSegment2D(
                        light.Position,
                        light.Position + Vector2.Normalize(intersectionCtx.UmbraIntersectionPoint - light.Position) * light.Range);                    
                    var penumbraSegment = fin.Side == Side.Left 
                        ? new LineSegment2D(fin.Vertex1.Position, fin.Vertex2.Position) 
                        : new LineSegment2D(fin.Vertex1.Position, fin.Vertex3.Position);

                    if (penumbraSegment.Intersects(segmentToTestAgainst))
                    {
                        //ClipMid(fin, intersectionCtx); // TODO: TEMP
                        break;
                    }
                }                

                // ADD TEXCOORDS, INTERPOLATE
                AddTexCoordsAndPopulateFinalVertices(fin);                

                if (intersectionsCount > 0)
                {
                    AttachInterpolatedVerticesToContext(fin, hullCtx);
                }

                // TRIANGULATE
                fin.Vertices.GetIndices(WindingOrder.Clockwise, fin.Indices);

                _vertices.AddRange(fin.FinalVertices);
                for (int i = 0; i < fin.Indices.Count; i++)
                {
                    fin.Indices[i] = fin.Indices[i] + _indexOffset;
                }
                _indices.AddRange(fin.Indices);
                _indexOffset += fin.FinalVertices.Count;

                _finPool.Release(fin);                
            }         
            
            // TODO: TEMP   
            //hullCtx.UmbraLeftProjectedVertex.TexCoord = hullCtx.UmbraRightProjectedVertex.TexCoord;

            _fins.Clear();
        }

        private void AttachInterpolatedVerticesToContext(PenumbraFin fin, HullContext hullCtx)
        {
            foreach (var vertex in fin.FinalVertices)
            {
                // We can populate only 1 vertex from a single fin.
                if (VectorUtil.NearEqual(vertex.Position, hullCtx.UmbraIntersectionContexts[0].UmbraLeftProjectedPoint))
                {
                    hullCtx.UmbraIntersectionContexts[0].UmbraLeftProjectedVertices.Add(vertex);
                    return;
                }
                if (VectorUtil.NearEqual(vertex.Position, hullCtx.UmbraIntersectionContexts[0].UmbraRightProjectedPoint))
                {
                    hullCtx.UmbraIntersectionContexts[0].UmbraRightProjectedVertices.Add(vertex);
                    return;
                }
                if (VectorUtil.NearEqual(vertex.Position, hullCtx.UmbraIntersectionContexts[0].UmbraIntersectionPoint))
                {
                    hullCtx.UmbraIntersectionContexts[0].UmbraIntersectionVertices.Add(vertex);
                }
            }
        }

        public void Build(Light light, LightVaos vaos)
        {
            if (_vertices.Count > 0 && _indices.Count > 0)
            {
                vaos.HasPenumbra = true;                
                vaos.PenumbraVao.SetVertices(_vertices);
                vaos.PenumbraVao.SetIndices(_indices);                
            } 
            else
            {
                vaos.HasPenumbra = false;
            }            
        }
        
        private PenumbraFin CreateFin(Light light, ref HullPointContext context, Hull hull, Side side)
        {
            PenumbraFin result = _finPool.Fetch();
            result.Clear();
            result.Side = side;            

            // FIND MAIN VERTICES
            PopulateMainVertices(result, light, ref context);
                
            if (light.ShadowType == ShadowType.Occluded)
            {
                // CLIP HULL VERTICES
                ClipHullFromFin(result, hull);
                // REORDER VERTICES FIN ORIGIN FIRST
                OrderFinVerticesOriginFirst(result);
            } 

            return result;
        }        

        private void PopulateMainVertices(PenumbraFin result, Light light, ref HullPointContext context)
        {
            // ROTATE A LITTLE BIT TOWARD UMBRA TO REMOVE 1 PX INACCURACIES/FLICKERINGS.
            Vector2 lightRightSideToCurrentDir = VectorUtil.Rotate(context.LightRightToPointDir, -MathHelper.ToRadians(DegreesToRotatePenumbraTowardUmbra));
            Vector2 lightLeftSideToCurrentDir = VectorUtil.Rotate(context.LightLeftToPointDir, MathHelper.ToRadians(DegreesToRotatePenumbraTowardUmbra));
            //Vector2 lightRightSideToCurrentDir = context.LightRightToPointDir;
            //Vector2 lightLeftSideToCurrentDir = context.LightLeftToPointDir;
            // CALCULATE RANGE.
            float range = light.Range / Vector2.Dot(context.LightToPointDir, lightRightSideToCurrentDir);

            //int outerTexCoord = context.IsInAnotherHull ? 1 : 0;
            int outerTexCoord = 0;

            result.Vertex1 = new VertexPosition2Texture(context.Point, new Vector2(0, 1));
            result.Vertex3 = new VertexPosition2Texture(
                context.LightRight + lightRightSideToCurrentDir * range,
                new Vector2(result.Side == Side.Left ? outerTexCoord : 1, 0));
            result.Vertex2 = new VertexPosition2Texture(
                context.LightLeft + lightLeftSideToCurrentDir * range,
                new Vector2(result.Side == Side.Left ? 1 : outerTexCoord, 0));

            result.Vertices.Add(result.Vertex1.Position);
            result.Vertices.Add(result.Vertex2.Position);
            result.Vertices.Add(result.Vertex3.Position);
        }

        private static void ClipHullFromFin(PenumbraFin result, Hull hull)
        {            
            Polygon.Clip(result.Vertices, hull.WorldPoints, result.Vertices);
        }

        private void OrderFinVerticesOriginFirst(PenumbraFin fin)
        {
            int index = 0;
            for (int i = 0; i < fin.Vertices.Count; i++)
            {
                Vector2 pos = fin.Vertices[i];
                if (VectorUtil.NearEqual(pos, fin.Vertex1.Position))
                {
                    index = i;
                    break;
                }
            }

            if (index != 0)
            {
                int vertexCount = fin.Vertices.Count;
                int numToShift = vertexCount - index;                
                fin.Vertices.ShiftRight<Vector2>(numToShift);                
             }
        }

        private void AddTexCoordsAndPopulateFinalVertices(PenumbraFin result)
        {
            foreach (Vector2 p in result.Vertices)
            {
                if (VectorUtil.NearEqual(p, result.Vertex1.Position))
                {
                    result.FinalVertices.Add(result.Vertex1);
                }
                else if (VectorUtil.NearEqual(p, result.Vertex2.Position))
                {
                    result.FinalVertices.Add(result.Vertex2);
                }
                else if (VectorUtil.NearEqual(p, result.Vertex3.Position))
                {
                    result.FinalVertices.Add(result.Vertex3);
                }
                else
                {
                    Vector2 point = p;                    
                    result.FinalVertices.Add(
                        VertexPosition2Texture.InterpolateTexCoord(ref result.Vertex1, ref result.Vertex2, ref result.Vertex3, ref point));
                }
            }
        }

        private  void ClipMid(PenumbraFin fin, UmbraIntersectionContext intersectionContext)
        {            
            if (fin.Side == Side.Left)
            {
                fin.Vertices.Insert(fin.Vertices.Count - 2, intersectionContext.UmbraIntersectionPoint);
                fin.Vertices[fin.Vertices.Count - 2] = intersectionContext.UmbraRightProjectedPoint;

                var v1 = VertexPosition2Texture.InterpolateTexCoord(ref fin.Vertex1, ref fin.Vertex2, ref fin.Vertex3, ref intersectionContext.UmbraIntersectionPoint); // TODO: experimental
                var v2 = VertexPosition2Texture.InterpolateTexCoord(ref fin.Vertex1, ref fin.Vertex2, ref fin.Vertex3, ref intersectionContext.UmbraRightProjectedPoint);
                intersectionContext.LeftVertices.Add(AntumbraSide.Create(v1, v2, fin.Vertex2)); // TODO: might be other way around
            }
            else
            {
                fin.Vertices.Insert(2, intersectionContext.UmbraLeftProjectedPoint);
                fin.Vertices[3] = intersectionContext.UmbraIntersectionPoint;

                var v1 = VertexPosition2Texture.InterpolateTexCoord(ref fin.Vertex1, ref fin.Vertex2, ref fin.Vertex3, ref intersectionContext.UmbraIntersectionPoint);
                var v2 = VertexPosition2Texture.InterpolateTexCoord(ref fin.Vertex1, ref fin.Vertex2, ref fin.Vertex3, ref intersectionContext.UmbraLeftProjectedPoint);
                intersectionContext.RightVertices.Add(AntumbraSide.Create(v1, v2, fin.Vertex3));
            }
        }

        private class PenumbraFin
        {
            public readonly FastList<VertexPosition2Texture> FinalVertices = new FastList<VertexPosition2Texture>();
            public readonly Polygon Vertices = new Polygon();
            public readonly FastList<int> Indices = new FastList<int>();

            public VertexPosition2Texture Vertex1; // hull point
            public VertexPosition2Texture Vertex2; // projected left or right
            public VertexPosition2Texture Vertex3; // projected left or right            
            public Side Side;            
 
            public void Clear()
            {                
                FinalVertices.Clear(true);
                Vertices.Clear(true);
                Indices.Clear(true);
            }
        }        
    }    
}