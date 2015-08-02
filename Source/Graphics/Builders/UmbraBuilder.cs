using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Collision;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class UmbraBuilder
    {
        private readonly HullList _hulls;

        private readonly List<int> _hullIndices = new List<int>();
        private readonly Polygon _hullVertices = new Polygon();
        
        private readonly FastList<Vector2> _vertices = new FastList<Vector2>();
        private readonly FastList<int> _indices = new FastList<int>();        

        private bool _isFirstSegment = true;
        private readonly List<HullPointContext> _firstSegmentBuffer = new List<HullPointContext>();
        //private readonly List<List<HullPointContext>> _segments = new List<List<HullPointContext>>();
        private readonly List<HullPointContext> _activeSegment = new List<HullPointContext>();

        private int _indexOffset;

        public UmbraBuilder(HullList hulls)
        {
            _hulls = hulls;
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
            //PointType type = GetPointType(ref context);            
            bool isLast = IsLastPoint(hull, ref context);
            //switch (type)
            switch (context.Side)
            {
                //case PointType.RightEdge:
                case Side.Right:                    
                    _isFirstSegment = false;
                    _activeSegment.Clear();
                    //_activeSegment = new List<HullPointContext>();
                    //_segments.Add(_activeSegment);
                    _activeSegment.Add(context);
                    if (isLast)
                        AppendFirstSegmentToActiveSegment();
                    break;
                //case PointType.Backward:
                case Side.Backward:
                    if (_isFirstSegment)
                        _firstSegmentBuffer.Add(context);                  
                    else
                        _activeSegment.Add(context);
                    if (isLast)
                        AppendFirstSegmentToActiveSegment();
                    break;
                //case PointType.LeftEdge:
                case Side.Left:
                    if (_isFirstSegment)
                        _firstSegmentBuffer.Add(context);
                    else
                        _activeSegment.Add(context);
                    _isFirstSegment = false;
                    // First segment is already handled.
                    break;
                //case PointType.Forward:
                case Side.Forward:
                    _isFirstSegment = false;
                    // First segment is already handled.
                    break;                    
            }
            //Logger.Write(type + " " + context.Position.ToString("0"));
        }

        public void ProcessHull(Light light, HullPart hull, ref HullContext hullCtx)
        {            
            // EACH CONVEX HULL HAS ONLY 1 SEGMENT. CONCAVE HULLS CAN HAVE MORE, BUT CURRENTLY NOT SUPPORTED.
            List<HullPointContext> segment = _activeSegment;
            //foreach (List<HullPointContext> segment in _segments)
            //{
                //if (segment.Count <= 1) continue;
            if (segment.Count <= 1) return;

            int startIndex = 0;
            Vector2 lightSideRight, lightSideToCurrentDirRight;
            do
            {
                Vector2 lightToCurrentDir;
                GetUmbraVectors(light, segment[startIndex].Position, +1f, out lightSideRight, out lightSideToCurrentDirRight, out lightToCurrentDir);
                Vector2 currentToNextDir = Vector2.Normalize(segment[startIndex + 1].Position - segment[startIndex].Position);                
                if (!VectorUtil.Intersects(lightToCurrentDir, lightSideToCurrentDirRight, currentToNextDir))
                {
                    // TEST LINE OF SIGHT
                    TestLineOfSight(light, hull, segment, startIndex, ref lightSideRight, ref lightSideToCurrentDirRight, lightToCurrentDir);
                    break;
                }
            } while (++startIndex < segment.Count - 1);

            int endIndex = segment.Count - 1;
            Vector2 lightSideLeft, lightSideToCurrentDirLeft;
            do
            {
                Vector2 lightToCurrentDir;
                GetUmbraVectors(light, segment[endIndex].Position, -1f, out lightSideLeft, out lightSideToCurrentDirLeft, out lightToCurrentDir);                    
                Vector2 currentToPreviousDir = Vector2.Normalize(segment[endIndex - 1].Position - segment[endIndex].Position);                
                if (!VectorUtil.Intersects(lightToCurrentDir, lightSideToCurrentDirLeft, currentToPreviousDir))
                {
                    // TEST LINE OF SIGHT
                    TestLineOfSight(light, hull, segment, endIndex, ref lightSideLeft, ref lightSideToCurrentDirLeft, lightToCurrentDir);
                    break;
                }
            } while (--endIndex >= 1);

            var line1 = new Line2D(lightSideRight, lightSideRight + lightSideToCurrentDirRight);
            var line2 = new Line2D(lightSideLeft, lightSideLeft + lightSideToCurrentDirLeft);

            Vector2 intersectionPos;
            bool linesIntersect = line1.Intersects(ref line2, out intersectionPos);

            var midDir = linesIntersect 
                ? Vector2.Normalize(intersectionPos - light.Position) 
                : Vector2.Normalize(lightSideToCurrentDirRight + lightSideToCurrentDirLeft);

            if (Vector2.Dot(midDir, lightSideToCurrentDirRight) < 0)
                midDir *= -1;

            Vector2 pointOnRange = light.Position + midDir * light.Range;

            bool areIntersectingInFrontOfLight = Vector2.DistanceSquared(intersectionPos, pointOnRange) < light.RangeSquared;

            Vector2 tangentDir = VectorUtil.Rotate90CW(midDir);

            Line2D tangentLine = new Line2D(pointOnRange, pointOnRange + tangentDir);

            Vector2 projectedPoint1;
            tangentLine.Intersects(ref line1, out projectedPoint1);
            Vector2 projectedPoint2;
            tangentLine.Intersects(ref line2, out projectedPoint2);            

            if (linesIntersect && areIntersectingInFrontOfLight)
            {
                bool areIntersectingInsideLightRange = Vector2.DistanceSquared(intersectionPos, light.Position) <
                                                        light.RangeSquared;
                                        
                if (areIntersectingInsideLightRange)
                {
                    hullCtx.UmbraIntersectionType = IntersectionType.IntersectsInsideLight;
                    _hullVertices.Add(intersectionPos);
                }
                else
                {
                    hullCtx.UmbraIntersectionType = IntersectionType.IntersectsOutsideLight;
                    _hullVertices.Add(projectedPoint1);
                    _hullVertices.Add(projectedPoint2);
                }                    

                hullCtx.UmbraIntersectionPoint = intersectionPos;
                hullCtx.UmbraRightProjectedPoint = projectedPoint1;
                hullCtx.UmbraLeftProjectedPoint = projectedPoint2;                    
            }
            else
            {
                _hullVertices.Add(projectedPoint1);
                _hullVertices.Add(projectedPoint2);                                 
            }
                

            // Add all the vertices that contain the segment on the hull.
            int numSegmentVertices = endIndex - startIndex + 1;
            for (int i = numSegmentVertices - 1; i >= 0; i--)
            //for (int i = 0; i > numSegmentVertices; i++)
            {
                Vector2 point = segment[startIndex + i].Position;
                _hullVertices.Add(point);
            }

            //Vector2[] outVertices;
            //int[] outIndices;
            //Triangulator.Triangulate(vertices.ToArray(), WindingOrder.CounterClockwise,
            //    WindingOrder.CounterClockwise, WindingOrder.Clockwise, out outVertices,
            //    out outIndices);            
            _hullIndices.Clear();
            _hullVertices.GetIndices(WindingOrder.Clockwise, _hullIndices);

            _vertices.AddRange(_hullVertices);            
            for (int i = 0; i < _hullIndices.Count; i++)
            {
                _hullIndices[i] = _hullIndices[i] + _indexOffset;
            }
            _indices.AddRange(_hullIndices);
            _indexOffset += _hullVertices.Count;
            //}

            _firstSegmentBuffer.Clear();
            _hullVertices.Clear();
            _isFirstSegment = true;            
            //_segments.Clear();
        }

        private void TestLineOfSight(Light light, HullPart hull, List<HullPointContext> segment, int startIndex, ref Vector2 lightSide, ref Vector2 lightSideToCurrentDir, Vector2 lightToCurrentDir)
        {
            for (int i = 0; i < _hulls.Count; i++)
            {
                HullPart otherHull = _hulls[i];
                if (otherHull == hull) continue;
                
                //var ray = new Ray2D(light.Position, lightToCurrentDir);
                var ray = new Ray2D(ref lightSide, ref lightSideToCurrentDir);
                float distance;
                if (ray.Intersects(otherHull.TransformedHullVertices, out distance))
                {
                    if (distance * distance - 0.1f <= Vector2.DistanceSquared(segment[startIndex].Position, light.Position))
                    {
                        lightSide = light.Position;
                        lightSideToCurrentDir = lightToCurrentDir;                        
                        break;
                    }
                }
            }
        }

        public void Build(Light light, LightVaos vaos)
        {
            if (_vertices.Count > 0 && _indices.Count > 0)
            {
                vaos.HasUmbra = true;                
                vaos.UmbraVao.SetVertices(_vertices);
                vaos.UmbraVao.SetIndices(_indices);                
            } 
            else
            {
                vaos.HasUmbra = false;
            }
        }

        private void GetUmbraVectors(Light light, Vector2 position, float project, out Vector2 lightSide, out Vector2 lightSideToCurrentDir, out Vector2 lightToCurrentDir)
        {
            lightToCurrentDir = Vector2.Normalize(position - light.Position);
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
