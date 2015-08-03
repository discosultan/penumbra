using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class PenumbraBuilder
    {
        private const float DegreesToRotatePenumbraTowardUmbra = 0.1f;

        private readonly FastList<VertexPosition2Texture> _vertices = new FastList<VertexPosition2Texture>();
        private readonly FastList<int> _indices = new FastList<int>();        
        private readonly Pool<PenumbraFin> _finPool = new Pool<PenumbraFin>();
        private readonly List<PenumbraFin> _fins = new List<PenumbraFin>();

        private int _indexOffset;

        public bool AddAdditionalFins { get; set; } = true;

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
        public void ProcessHullPoint(Light light, Hull hull, ref HullPointContext context)
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
            else if (_addLast && context.Index == hull.TransformedPoints.Count - 1)
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
                if (fin.Intersects && AddAdditionalFins)
                {
                    switch (fin.Side)
                    {
                        case Side.Right:                        
                            if (context.Index == hull.TransformedPoints.Count - 1)
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

        public void ProcessHull(Light light, Hull hull, ref HullContext hullCtx)
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

                if (hullCtx.UmbraIntersectionType == IntersectionType.IntersectsInsideLight && fin.IsCreatedByIntersection)
                {                    
                    AttachInterpolatedVerticesToContext(fin, ref hullCtx);                    
                }

                // TRIANGULATE
                TriangulateFin(fin);

                //// TODO: TEMP
                //if (fin.IsCreatedByIntersection)
                //{
                    _vertices.AddRange(fin.FinalVertices);
                    for (int i = 0; i < fin.Indices.Count; i++)
                    {
                        fin.Indices[i] = fin.Indices[i] + _indexOffset;
                    }
                    _indices.AddRange(fin.Indices);
                    _indexOffset += fin.FinalVertices.Count;
                //}

                _finPool.Release(fin);                
            }         
            
            // TODO: TEMP   
            hullCtx.UmbraLeftProjectedVertex.TexCoord = hullCtx.UmbraRightProjectedVertex.TexCoord;

            _addNext = false;
            _addLast = false;
            _fins.Clear();
        }

        private void AttachInterpolatedVerticesToContext(PenumbraFin fin, ref HullContext hullCtx)
        {
            foreach (var vertex in fin.FinalVertices)
            {
                // We can populate only 1 vertex from a single fin.
                if (VectorUtil.NearEqual(vertex.Position, hullCtx.UmbraLeftProjectedPoint))
                {
                    hullCtx.UmbraLeftProjectedVertex = vertex;                    
                    return;
                }
                if (VectorUtil.NearEqual(vertex.Position, hullCtx.UmbraRightProjectedPoint))
                {
                    hullCtx.UmbraRightProjectedVertex = vertex;
                    return;
                }
                if (VectorUtil.NearEqual(vertex.Position, hullCtx.UmbraIntersectionPoint))
                {
                    hullCtx.UmbraIntersectionVertex = vertex;                    
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
            // TODO: TEMP
            vaos.HasPenumbra = false;
        }
        
        private PenumbraFin CreateFin(Light light, ref HullPointContext context, Hull hull, Side side, bool testIntersection = false)
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

        private bool TestIntersection(PenumbraFin result, Hull hull, ref HullPointContext context, ref PenumbraFinContext finContext)
        {
            var positions = hull.TransformedPoints;

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

        private static void ClipHullFromFin(PenumbraFin result, Hull hull, ref HullPointContext context, ref PenumbraFinContext finContext)
        {            
            Polygon.Clip(result.Vertices, hull.TransformedPoints, result.Vertices);
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
                int  vertexCount = fin.Vertices.Count;
                int numToShift = vertexCount - index;                
                fin.Vertices.ShiftRight<Vector2>(numToShift);                
             }
        }

        private void AddTexCoords(PenumbraFin result)
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
                    Vector3 barycentricCoords;
                    VectorUtil.Barycentric(
                        ref point, 
                        ref result.Vertex1.Position,                         
                        ref result.Vertex2.Position,
                        ref result.Vertex3.Position, 
                        out barycentricCoords);
                    Vector2 interpolatedTexCoord =
                        result.Vertex1.TexCoord * barycentricCoords.X +
                        result.Vertex2.TexCoord * barycentricCoords.Y +
                        result.Vertex3.TexCoord * barycentricCoords.Z;

                    result.FinalVertices.Add(new VertexPosition2Texture(
                        p,
                        interpolatedTexCoord));                        
                }
            }
        }

        private void TriangulateFin(PenumbraFin result)
        {            
            result.Vertices.GetIndices(WindingOrder.Clockwise, result.Indices);            
        }

        private static void ClipMid(PenumbraFin fin, Light light, ref HullContext hullCtx)
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
            public readonly Polygon Vertices = new Polygon();
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