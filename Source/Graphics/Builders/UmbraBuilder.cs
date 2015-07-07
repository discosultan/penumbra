using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Triangulation;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class UmbraBuilder
    {
        private readonly ArrayPool<Vector2> _vertexArrayPool;
        private readonly ArrayPool<int> _indexArrayPool;

        private readonly List<Vector2> _vertices = new List<Vector2>();
        private readonly List<int> _indices = new List<int>();        

        private bool _isFirstSegment = true;
        private readonly List<HullPointContext> _firstSegmentBuffer = new List<HullPointContext>();
        private readonly List<List<HullPointContext>> _segments = new List<List<HullPointContext>>();
        private List<HullPointContext> _activeSegment;

        private int _indexOffset;

        public UmbraBuilder(ArrayPool<Vector2> vertexArrayPool, ArrayPool<int> indexArrayPool)
        {
            _vertexArrayPool = vertexArrayPool;
            _indexArrayPool = indexArrayPool;
        }

        public void PreProcess()
        {
            _indexOffset = 0;
            _vertices.Clear();
            _indices.Clear();            
            //_firstSegmentBuffer.Clear();
            //_isFirstSegment = true;
            //_segments.Clear();            
        }
        
        public void ProcessHullPoint(Light light, HullPart hull, ref HullPointContext context)
        {            
            PointType type = GetPointType(ref context);
            bool isLast = IsLastPoint(hull, ref context);
            switch (type)
            {
                case PointType.RightEdge:
                    _isFirstSegment = false;
                    _activeSegment = new List<HullPointContext>();
                    _segments.Add(_activeSegment);
                    _activeSegment.Add(context);
                    if (isLast)
                        AppendFirstSegmentToActiveSegment();
                    break;
                case PointType.Backward:
                    if (_isFirstSegment)
                        _firstSegmentBuffer.Add(context);                  
                    else
                        _activeSegment.Add(context);
                    if (isLast)
                        AppendFirstSegmentToActiveSegment();
                    break;
                case PointType.LeftEdge:
                    if (_isFirstSegment)
                        _firstSegmentBuffer.Add(context);
                    else
                        _activeSegment.Add(context);
                    _isFirstSegment = false;
                    // First segment is already handled.
                    break;
                case PointType.Forward:
                    _isFirstSegment = false;
                    // First segment is already handled.
                    break;                    
            }
            //Logger.Write(type + " " + context.Position.ToString("0"));
        }

        public void ProcessHull(Light light, HullPart hull, ref HullContext hullCtx)
        {            
            foreach (List<HullPointContext> segment in _segments)
            {
                if (segment.Count <= 1) continue;

                int startIndex = 0;
                Vector2 lightSide1, lightSideToCurrentDir1;
                do
                {
                    GetUmbraVectors(light, segment[startIndex].Position, +1f, out lightSide1, out lightSideToCurrentDir1);
                    Vector2 lightToCurrentDir = Vector2.Normalize(segment[startIndex].Position - light.Position);
                    Vector2 currentToNextDir = Vector2.Normalize(segment[startIndex + 1].Position - segment[startIndex].Position);
                    if (!VectorUtil.Intersects(lightToCurrentDir, lightSideToCurrentDir1, currentToNextDir))
                    {
                        break;
                    }
                } while (++startIndex < segment.Count - 1);
                int endIndex = segment.Count - 1;
                Vector2 lightSide2, lightSideToCurrentDir2;
                do
                {
                    GetUmbraVectors(light, segment[endIndex].Position, -1f, out lightSide2, out lightSideToCurrentDir2);
                    Vector2 lightToCurrentDir = Vector2.Normalize(segment[endIndex].Position - light.Position);
                    Vector2 currentToPreviousDir = Vector2.Normalize(segment[endIndex - 1].Position - segment[endIndex].Position);
                    if (!VectorUtil.Intersects(lightToCurrentDir, lightSideToCurrentDir2, currentToPreviousDir))
                    {
                        break;
                    }
                } while (--endIndex >= 1);

                float range = (light.Range + light.Radius) / Vector2.Dot(lightSideToCurrentDir1, Vector2.Normalize(lightSideToCurrentDir1 + lightSideToCurrentDir2));

                Vector2 rightProjectedPos = lightSide1 + lightSideToCurrentDir1 * range;
                Vector2 leftProjectedPos = lightSide2 + lightSideToCurrentDir2 * range;

                Vector2 intersectionPos;
                bool lineIntersects = VectorUtil.LineIntersect(
                    ref lightSide1,
                    ref rightProjectedPos,
                    ref lightSide2,
                    ref leftProjectedPos,
                    out intersectionPos);

                var vertices = new Polygon(WindingOrder.CounterClockwise);

                if (lineIntersects)
                {
                    Logger.Write("Projected lines intersect");
                    vertices.Add(intersectionPos);
                    hullCtx.UmbraIntersectionType = Vector2.DistanceSquared(intersectionPos, light.Position) < light.RangeSquared 
                        ? IntersectionType.IntersectsInsideLight 
                        : IntersectionType.IntersectsOutsideLight;

                    hullCtx.UmbraIntersectionPoint = intersectionPos;
                }
                else
                {
                    vertices.Add(rightProjectedPos);
                    vertices.Add(leftProjectedPos);                                 
                }
                // Add all the vertices that contain the segment on the hull.
                int numSegmentVertices = endIndex - startIndex + 1;
                for (int i = numSegmentVertices - 1; i >= 0; i--)
                //for (int i = 0; i > numSegmentVertices; i++)
                {
                    Vector2 point = segment[startIndex + i].Position;
                    vertices.Add(point);
                }

                //Vector2[] outVertices;
                //int[] outIndices;
                //Triangulator.Triangulate(vertices.ToArray(), WindingOrder.CounterClockwise,
                //    WindingOrder.CounterClockwise, WindingOrder.Clockwise, out outVertices,
                //    out outIndices);
                var indices = new List<int>();
                vertices.GetIndices(WindingOrder.Clockwise, indices);

                _vertices.AddRange(vertices);                
                _indices.AddRange(indices.Select(x => _indexOffset + x));
                _indexOffset += vertices.Count;
            }

            _firstSegmentBuffer.Clear();
            _isFirstSegment = true;
            _segments.Clear();
        }

        public void Build(Light light, LightVaos vaos)
        {
            if (_vertices.Count > 0 && _indices.Count > 0)
            {
                vaos.HasUmbra = true;
                Vector2[] umbraVertices = _vertices.ToArrayFromPool(_vertexArrayPool);
                int[] umbraIndices = _indices.ToArrayFromPool(_indexArrayPool);
                vaos.UmbraVao.SetVertices(umbraVertices);
                vaos.UmbraVao.SetIndices(umbraIndices);
                _vertexArrayPool.Release(umbraVertices);
                _indexArrayPool.Release(umbraIndices);
            } 
            else
            {
                vaos.HasUmbra = false;
            }
        }

        private static void GetUmbraVectors(Light light, Vector2 position, float project, out Vector2 lightSide, out Vector2 lightSideToCurrentDir)
        {
            Vector2 lightToCurrentDir = Vector2.Normalize(position - light.Position);
            Vector2 lightToCurrent90CWDir = VectorUtil.Rotate90CW(lightToCurrentDir);

            lightSide = light.Position + lightToCurrent90CWDir * light.Radius * project;
            lightSideToCurrentDir = Vector2.Normalize(position - lightSide);
        }

        private void AppendFirstSegmentToActiveSegment()
        {
            foreach (HullPointContext bufferedContext in _firstSegmentBuffer)
            {
                _activeSegment.Add(bufferedContext);
            }            
        }

        private bool IsLastPoint(HullPart hull, ref HullPointContext context)
        {
            return context.Index >= hull.TransformedHullVertices.Count - 1;
        }

        private PointType GetPointType(ref HullPointContext context)
        {
            float dot1, dot2;
            //if (context.Normals.Convex)
            //{
                dot1 = context.Dot1;
                dot2 = context.Dot2;
            //} 
            //else
            //{
            //    dot2 = context.Dot1;
            //    dot1 = context.Dot2;    
            //}

            if (dot1 < 0 && dot2 >= 0)
                return PointType.RightEdge;
            if (dot1 >= 0 && dot2 < 0)
                return PointType.LeftEdge;
            if (dot1 >= 0 && dot2 >= 0)
                return PointType.Backward;
            return PointType.Forward;
        }

        private enum PointType
        {
            None,
            LeftEdge,
            Backward,
            Forward,
            RightEdge            
        }
    }    
}
