using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Collision;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class UmbraBuilder3
    {
        private readonly Pool<FastList<HullPointContext>> _segmentPool = new Pool<FastList<HullPointContext>>();

        private readonly FastList<FastList<HullPointContext>> _segments = new FastList<FastList<HullPointContext>>();
        private readonly List<HullPointContext> _skippedContexts = new List<HullPointContext>();

        private readonly FastList<int> _hullIndices = new FastList<int>();
        private readonly Polygon _hullVertices = new Polygon();

        private readonly FastList<Vector2> _vertices = new FastList<Vector2>();
        private readonly FastList<int> _indices = new FastList<int>();

        private int _indexOffset;

        public void PreProcess()
        {
            _indexOffset = 0;
            _vertices.Clear();
            _indices.Clear();
        }
        
        public void ProcessHull(Light light, HullContext hullCtx)
        {
            PopulateSegments(hullCtx);
            PopulateVertices(light, hullCtx);
            ReleaseSegments(_segments);                
        }

        private void ReleaseSegments(FastList<FastList<HullPointContext>> segments)
        {
            foreach (var segment in segments)            
                _segmentPool.Release(segment);            
            segments.Clear();
        }
        
        private void PopulateSegments(HullContext hullCtx)
        {
            var points = hullCtx.PointContexts;
            int count = points.Count;
            int indexer = FindStartIndex(hullCtx);
            var activeSegment = CreateSegment();
            _skippedContexts.Clear();
            bool segmentIsActive = true;            
            for (int i = 0; i < count; i++)
            {                
                var ctx = points[indexer];
                if (segmentIsActive)
                {                    
                    if (ctx.LeftSide == Side.Left && ctx.IsConvex)
                    {
                        segmentIsActive = false;                        
                        AddSegmentToSegments(activeSegment);
                    }
                    else if (ctx.RightSide == Side.Right && ctx.IsConvex && activeSegment.Count > 0)
                    {
                        HullPointContext prevStartCtx = activeSegment[0];
                        if (VectorUtil.IsADirectingRightFromB(ref prevStartCtx.LightRightToPointDir,
                            ref ctx.LightRightToPointDir))
                        {
                            // Do something?                                                        
                        }
                        else
                        {
                            // Remove til last left
                            for (int j = activeSegment.Count - 1; j >= 0; j--)
                            {
                                if (activeSegment[j].LeftSide != Side.Left)
                                {
                                    activeSegment.RemoveAt(j);
                                }
                                else
                                {
                                    break;
                                }
                            }

                            AddSegmentToSegments(activeSegment);                                
                            activeSegment = CreateSegment();
                        }
                    }
                    activeSegment.Add(ctx);
                }
                else
                {
                    if (ctx.RightSide == Side.Right && ctx.IsConvex)
                    {
                        segmentIsActive = true;
                        // Check if previous segment should be merged with new one.
                        HullPointContext prevStartCtx = activeSegment[0];                        
                        if (VectorUtil.IsADirectingRightFromB(ref prevStartCtx.LightRightToPointDir, ref ctx.LightRightToPointDir))
                        {
                            // Do something?                            
                            HullPointContext prevEndCtx = activeSegment[activeSegment.Count - 1];
                            if (VectorUtil.IsADirectingRightFromB(ref prevEndCtx.LightLeftToPointDir,
                                ref ctx.LightRightToPointDir))
                            {
                                activeSegment.AddRange(_skippedContexts);
                                _skippedContexts.Clear();
                            }
                            else if (_skippedContexts.Count > 0)
                            {
                                activeSegment = CreateSegment();
                            }
                        }                        
                        else
                        {
                            activeSegment = CreateSegment();
                        }
                        activeSegment.Add(ctx);
                        _skippedContexts.Clear();
                    }
                    else if (ctx.LeftSide == Side.Left && ctx.IsConvex)
                    {
                        HullPointContext prevEndCtx = activeSegment[activeSegment.Count - 1];
                        if (VectorUtil.IsADirectingRightFromB(ref prevEndCtx.LightLeftToPointDir,
                            ref ctx.LightLeftToPointDir))
                        {                            
                            activeSegment.AddRange(_skippedContexts);
                            activeSegment.Add(ctx);
                            _skippedContexts.Clear();
                        }
                        else
                        {
                            _skippedContexts.Add(ctx);
                            
                            activeSegment = CreateSegment();
                            bool startAdding = false;
                            foreach (var skippedCtx in _skippedContexts)
                            {
                                if (skippedCtx.RightSide == Side.Right)
                                {
                                    startAdding = true;
                                }
                                if (startAdding)
                                {
                                    activeSegment.Add(skippedCtx);
                                }
                            }                            
                            _segments.Add(activeSegment);
                            _skippedContexts.Clear();                            
                        }
                    }
                    else
                    {
                        _skippedContexts.Add(ctx);
                    }
                }

                indexer = points.NextIndex(indexer);
            }

            if (segmentIsActive)
            {
                var activeCtx = activeSegment[0];
                var firstCtx = _segments[0][0];
                if (VectorUtil.IsADirectingRightFromB(
                    ref activeCtx.LightRightToPointDir,
                    ref firstCtx.LightRightToPointDir))
                {
                    _segments[0].InsertRange(0, activeSegment);
                }
                else
                {
                    _segments.Add(activeSegment);
                }
            }

            // TODO: TEMP
            for (int i = 0, j = 1; j < _segments.Count; i++, j++)
            {
                Debug.Assert(_segments[i] != _segments[j]);
            }
        }        

        private static int FindStartIndex(HullContext hullCtx)
        {
            var points = hullCtx.PointContexts;
            int count = points.Count;            
            for (int i = 0; i < count; i++)
            {
                if (points[i].RightSide == Side.Right && points[i].IsConvex)
                    return i;
            }
            return -1;                       
        }

        private FastList<HullPointContext> CreateSegment()
        {
            var segment = _segmentPool.Fetch();
            segment.Clear(true);
            return segment;
        }

        private void AddSegmentToSegments(FastList<HullPointContext> segment)
        {
            if (!_segments.Contains(segment))
                _segments.Add(segment);
        }

        private void PopulateVertices(Light light, HullContext hullCtx)
        {
            foreach (var segment in _segments)
            {
                if (segment.Count <= 1) continue;

                // NEXT PHASE

                var right = segment[0];
                var left = segment[segment.Count - 1];
                var line1 = new Line2D(right.LightRight, right.LightRight + right.LightRightToPointDir);
                var line2 = new Line2D(left.LightLeft, left.LightLeft + left.LightLeftToPointDir);

                Vector2 intersectionPos;
                var linesIntersect = line1.Intersects(ref line2, out intersectionPos);

                var midDir = linesIntersect
                    ? Vector2.Normalize(intersectionPos - light.Position)
                    : Vector2.Normalize(right.LightRightToPointDir + left.LightLeftToPointDir);

                if (Vector2.Dot(midDir, right.LightRightToPointDir) < 0)
                    midDir *= -1;

                var pointOnRange = light.Position + midDir * light.Range;

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
                    bool areIntersectingInsideLightRange = Vector2.DistanceSquared(intersectionPos, light.Position) <
                                                          light.RangeSquared;

                    if (areIntersectingInsideLightRange)
                    {
                        hullCtx.UmbraIntersectionContexts.Add(new UmbraIntersectionContext
                        {
                            UmbraIntersectionPoint = intersectionPos,
                            UmbraRightProjectedPoint = projectedPoint1,
                            UmbraLeftProjectedPoint = projectedPoint2
                        });                        
                        _hullVertices.Add(intersectionPos);
                    }
                    else
                    {                        
                        _hullVertices.Add(projectedPoint1);
                        _hullVertices.Add(projectedPoint2);
                    }                    
                }
                else
                {
                    _hullVertices.Add(projectedPoint1);
                    _hullVertices.Add(projectedPoint2);
                }


                // Add all the vertices that contain the segment on the hull.
                var numSegmentVertices = segment.Count;
                for (var i = numSegmentVertices - 1; i >= 0; i--)
                {
                    var point = segment[i].Point;
                    _hullVertices.Add(point);
                }
                
                _hullVertices.GetIndices(WindingOrder.Clockwise, _hullIndices);

                _vertices.AddRange(_hullVertices);
                for (var i = 0; i < _hullIndices.Count; i++)
                {
                    _hullIndices[i] = _hullIndices[i] + _indexOffset;
                }
                _indices.AddRange(_hullIndices);
                _indexOffset += _hullVertices.Count;

                _hullVertices.Clear(true);
                _hullIndices.Clear(true);
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
    }
}
