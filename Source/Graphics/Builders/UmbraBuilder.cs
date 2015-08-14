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
        private readonly List<List<HullPointContext>> _segments = new List<List<HullPointContext>>();
        private List<HullPointContext> _activeSegment = new List<HullPointContext>();

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

        private bool _segmentActive;
        public void ProcessHullPoint(Light light, Hull hull, ref HullPointContext context)
        {
            var isLast = context.Index >= hull.TransformedPoints.Count - 1;
            switch (context.Side)
            {
                case Side.Right:
                    //_segmentActive = true;                        
                    _activeSegment = new List<HullPointContext>();
                    _segments.Add(_activeSegment);
                    _activeSegment.Add(context);
                    if (isLast)
                        AppendFirstSegmentToActiveSegment();
                    if (_segments.Count >= 2)
                    {
                        // Merge with previous if intersect  
                        var prevSegment = _segments[_segments.Count - 2];
                        var prevCtx = prevSegment[0];
                        //var ray1 = new Ray2D(lastCtx.Position, lastCtx.LightToPointDir);
                        //var ray2 = new Ray2D(context.Position, context.LightToPointDir);
                        //if (ray1.Intersects(ref ray2))
                        //{
                        //    _activeSegment.AddRange(prevSegment);
                        //    _segments.Remove(prevSegment);
                        //}
                        if (VectorUtil.IsADirectingRightFromB(ref prevCtx.LightRightToPointDir, ref context.LightRightToPointDir))
                        {
                            _activeSegment.InsertRange(0, prevSegment);
                            _segments.Remove(prevSegment);
                        }
                    }
                    _isFirstSegment = false;
                    break;
                case Side.Backward:
                    if (_isFirstSegment)
                        _firstSegmentBuffer.Add(context);
                    else
                        _activeSegment.Add(context);
                    if (isLast)
                        AppendFirstSegmentToActiveSegment();
                    break;
                case Side.Left:
                    //_segmentActive = false;
                    if (_isFirstSegment)
                        _firstSegmentBuffer.Add(context);
                    else
                        _activeSegment.Add(context);
                    _isFirstSegment = false;
                    if (isLast) // experimental
                    {
                        AppendFirstSegmentToActiveSegment();
                        // Merge with next if intersect
                    }
                    // First segment is already handled.
                    break;
                case Side.Forward:
                    _isFirstSegment = false;
                    //if (_segmentActive)
                    //{
                    //    _activeSegment.Add(context);
                    //}
                    // First segment is already handled.
                    break;
            }            
        }

        public void ProcessHull(Light light, Hull hull, ref HullContext hullCtx)
        {                        
            foreach (var segment in _segments)
            {
                if (segment.Count <= 1) continue;                

                var startIndex = 0;
                Vector2 lightSideRight, lightSideToCurrentDirRight;
                bool valid = false;
                do
                {
                    Vector2 lightRightToCurrentDir;
                    GetUmbraVectors(light, segment[startIndex].Point, +1f, out lightSideRight,
                        out lightSideToCurrentDirRight, out lightRightToCurrentDir);
                    var currentToNextDir =
                        Vector2.Normalize(segment[startIndex + 1].Point - segment[startIndex].Point);
                    if (!VectorUtil.Intersects(lightRightToCurrentDir, lightSideToCurrentDirRight, currentToNextDir))
                    {
                        valid = true;
                        break;
                    }
                } while (++startIndex < segment.Count - 1);

                if (!valid) continue;
                valid = false;

                var endIndex = segment.Count - 1;
                Vector2 lightSideLeft, lightSideToCurrentDirLeft;
                do
                {
                    Vector2 lightLeftToCurrentDir;
                    GetUmbraVectors(light, segment[endIndex].Point, -1f, out lightSideLeft,
                        out lightSideToCurrentDirLeft, out lightLeftToCurrentDir);
                    var currentToPreviousDir =
                        Vector2.Normalize(segment[endIndex - 1].Point - segment[endIndex].Point);
                    if (!VectorUtil.Intersects(lightLeftToCurrentDir, lightSideToCurrentDirLeft, currentToPreviousDir))
                    {
                        valid = true;
                        break;
                    }
                } while (--endIndex >= 1) ;

                if (!valid) continue;

                // NEXT PHASE

                var line1 = new Line2D(lightSideRight, lightSideRight + lightSideToCurrentDirRight);
                var line2 = new Line2D(lightSideLeft, lightSideLeft + lightSideToCurrentDirLeft);

                Vector2 intersectionPos;
                var linesIntersect = line1.Intersects(ref line2, out intersectionPos);

                var midDir = linesIntersect
                    ? Vector2.Normalize(intersectionPos - light.Position)
                    : Vector2.Normalize(lightSideToCurrentDirRight + lightSideToCurrentDirLeft);

                if (Vector2.Dot(midDir, lightSideToCurrentDirRight) < 0)
                    midDir *= -1;

                var pointOnRange = light.Position + midDir*light.Range;

                var areIntersectingInFrontOfLight = Vector2.DistanceSquared(intersectionPos, pointOnRange) <
                                                    light.RangeSquared;

                var tangentDir = VectorUtil.Rotate90CW(midDir);

                var tangentLine = new Line2D(pointOnRange, pointOnRange + tangentDir);

                Vector2 projectedPoint1;
                tangentLine.Intersects(ref line1, out projectedPoint1);
                Vector2 projectedPoint2;
                tangentLine.Intersects(ref line2, out projectedPoint2);

                if (linesIntersect && areIntersectingInFrontOfLight)
                {
                    var areIntersectingInsideLightRange = Vector2.DistanceSquared(intersectionPos, light.Position) <
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
                var numSegmentVertices = endIndex - startIndex + 1;
                for (var i = numSegmentVertices - 1; i >= 0; i--)
                {
                    var point = segment[startIndex + i].Point;
                    _hullVertices.Add(point);
                }

                //Vector2[] outVertices;
                //int[] outIndices;
                //Triangulator.Triangulate(vertices.ToArray(), WindingOrder.CounterClockwise,
                //    WindingOrder.CounterClockwise, WindingOrder.Clockwise, out outVertices,
                //    out outIndices);                            
                _hullVertices.GetIndices(WindingOrder.Clockwise, _hullIndices);

                _vertices.AddRange(_hullVertices);
                for (var i = 0; i < _hullIndices.Count; i++)
                {
                    _hullIndices[i] = _hullIndices[i] + _indexOffset;
                }
                _indices.AddRange(_hullIndices);
                _indexOffset += _hullVertices.Count;
            }

            _firstSegmentBuffer.Clear();
            _hullVertices.Clear();
            _hullIndices.Clear();
            _isFirstSegment = true;
            _segments.Clear();
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

        private void GetUmbraVectors(Light light, Vector2 position, float project, out Vector2 lightSide,
            out Vector2 lightSideToCurrentDir, out Vector2 lightToCurrentDir)
        {
            lightToCurrentDir = Vector2.Normalize(position - light.Position);
            var lightToCurrent90CWDir = VectorUtil.Rotate90CW(lightToCurrentDir);

            lightSide = light.Position + lightToCurrent90CWDir*light.Radius*project;
            lightSideToCurrentDir = Vector2.Normalize(position - lightSide);
        }

        private void AppendFirstSegmentToActiveSegment()
        {
            _activeSegment.AddRange(_firstSegmentBuffer);
        }
    }
}
