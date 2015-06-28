using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Triangulation;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Builders
{
    internal class CPUUmbraBuilder
    {
        private readonly ArrayPool<Vector2> _vertexArrayPool;
        private readonly ArrayPool<int> _indexArrayPool;

        private readonly List<Vector2> _vertices = new List<Vector2>();
        private readonly List<int> _indices = new List<int>();        

        private bool _isFirstSegment;
        private readonly List<PointProcessingContext> _firstSegmentBuffer = new List<PointProcessingContext>();
        private readonly List<List<PointProcessingContext>> _segments = new List<List<PointProcessingContext>>();
        private List<PointProcessingContext> _activeSegment;

        public CPUUmbraBuilder(ArrayPool<Vector2> vertexArrayPool, ArrayPool<int> indexArrayPool)
        {
            _vertexArrayPool = vertexArrayPool;
            _indexArrayPool = indexArrayPool;
        }

        public void PreProcess()
        {            
            _vertices.Clear();
            _indices.Clear();            
            _firstSegmentBuffer.Clear();
            _isFirstSegment = true;
            _segments.Clear();            
        }
        
        public void ProcessHullPoint(Light light, CPUHullPart hull, ref PointProcessingContext context)
        {            
            PointType type = GetPointType(ref context);
            bool isLast = IsLastPoint(hull, ref context);
            switch (type)
            {
                case PointType.LeftEdge:
                    _isFirstSegment = false;
                    _activeSegment = new List<PointProcessingContext>();
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
                case PointType.RightEdge:
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

        public void ProcessHull(Light light, CPUHullPart hull)
        {
            _firstSegmentBuffer.Clear();
            _isFirstSegment = true;
            //Logger.Write("Hull processed");
        }

        public void Build(Light light, LightVaos vaos)
        {
            int indexOffset = 0;
            foreach (List<PointProcessingContext> segment in _segments)
            {
                if (segment.Count <= 1) continue;

                int startIndex = 0;
                Vector2 lightSide1, lightSideToCurrentDir1;
                do
                {
                    GetUmbraVectors(light, segment[startIndex].Position, -1f, out lightSide1, out lightSideToCurrentDir1);
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
                    GetUmbraVectors(light, segment[endIndex].Position, +1f, out lightSide2, out lightSideToCurrentDir2);
                    Vector2 lightToCurrentDir = Vector2.Normalize(segment[endIndex].Position - light.Position);
                    Vector2 currentToPreviousDir = Vector2.Normalize(segment[endIndex - 1].Position - segment[endIndex].Position);
                    if (!VectorUtil.Intersects(lightToCurrentDir, lightSideToCurrentDir2, currentToPreviousDir))
                    {
                        break;
                    }
                } while (--endIndex >= 1);

                float range = (light.Range + light.Radius) / Vector2.Dot(lightSideToCurrentDir1, Vector2.Normalize(lightSideToCurrentDir1 + lightSideToCurrentDir2));
                
                Vector2 leftProjectedPos = lightSide1 + lightSideToCurrentDir1 * range;
                Vector2 rightProjectedPos = lightSide2 + lightSideToCurrentDir2 * range;

                //Vector2 intersectionPos;
                //Vector3 temp;
                //bool lineIntersects; 
                //var r1 = new Ray(new Vector3(lightSide1.X, lightSide1.Y, 0),
                //    new Vector3(lightSideToCurrentDir1.X, lightSideToCurrentDir1.Y, 0));
                //var r2 = new Ray(new Vector3(lightSide2.X, lightSide2.Y, 0),
                //    new Vector3(lightSideToCurrentDir2.X, lightSideToCurrentDir2.Y, 0));
                //lineIntersects = r1.Intersects(ref r2, out temp);
                //intersectionPos = new Vector2(temp.X, temp.Y);

                Vector2 intersectionPos;                
                bool lineIntersects = VectorUtil.LineIntersect(
                    ref lightSide1,
                    ref leftProjectedPos,
                    ref lightSide2,
                    ref rightProjectedPos,
                    out intersectionPos);

                // TRIANGULATOR

                //segment.Add(rightProjectedPos);
                //segment.Add(leftProjectedPos);

                //int numVertices = endIndex - startIndex + 1 + 2;
                //Vector2[] inputVertices = new Vector2[numVertices];
                //segment.CopyTo(startIndex, inputVertices, 0, numVertices);
                //Vector2[] outputVertices;
                //int[] outputIndices;
                //Triangulator.Triangulate(
                //    inputVertices,
                //    Triangulation.WindingOrder.CounterClockwise,
                //    Triangulation.WindingOrder.CounterClockwise,
                //    Triangulation.WindingOrder.Clockwise,
                //    out outputVertices,
                //    out outputIndices);

                //_vertices.AddRange(outputVertices.Select(x => new VertexPosition2(x)));
                //int offset = indexOffset;
                //_indices.AddRange(outputIndices.Select(x => offset + x));
                //indexOffset += outputVertices.Length;

                // NON TRIANGULATOR

                //lineIntersects = false;
                //if (lineIntersects)
                //{
                //    if (Vector2.Distance(intersectionPos, light.Position) < light.Radius)
                //    {
                //        lineIntersects = false;
                //    }
                //}

                List<Vector2> vertices = new List<Vector2>();

                if (lineIntersects)
                {
                    Logger.Write("Projected lines intersect");
                    //_vertices.Add(intersectionPos);
                    vertices.Add(intersectionPos);
                } 
                else
                {
                    vertices.Add(rightProjectedPos);
                    vertices.Add(leftProjectedPos);
                    //_vertices.Add(leftProjectedPos);
                    //_vertices.Add(rightProjectedPos);
                }
                // Add all the vertices that contain the segment on the hull.
                int numSegmentVertices = endIndex - startIndex + 1;
                for (int i = 0; i < numSegmentVertices; i++)
                {
                    Vector2 point = segment[startIndex + i].Position;
                    //_vertices.Add(point);
                    vertices.Add(point);
                    //_indices.Add(indexOffset + (i + 2));
                    //_indices.Add(indexOffset);
                    //if (i >= numSegmentVertices - 1) // Is last point?
                    //{
                    //    if (!lineIntersects)
                    //    {
                    //        _indices.Add(indexOffset + 1);
                    //    }
                    //}
                    //else
                    //{
                    //    _indices.Add(indexOffset + (i + 2) + 1);
                    //}
                }

                Vector2[] outVertices;
                int[] outIndices;
                Triangulator.Triangulate(vertices.ToArray(), WindingOrder.CounterClockwise,
                    WindingOrder.CounterClockwise, WindingOrder.Clockwise, out outVertices,
                    out outIndices);

                _vertices.AddRange(outVertices);
                int offset = indexOffset;
                _indices.AddRange(outIndices.Select(x => offset + x));
                indexOffset += outVertices.Length;

                //indexOffset += numSegmentVertices + (lineIntersects ? 1 : 2);
            }

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
            Vector2 lightToCurrentDir;
            Vector2 lightToCurrent90CWDir;
            lightToCurrentDir = Vector2.Normalize(position - light.Position);
            lightToCurrent90CWDir = VectorUtil.Rotate90CW(lightToCurrentDir);

            lightSide = light.Position + lightToCurrent90CWDir * light.Radius * project;
            lightSideToCurrentDir = Vector2.Normalize(position - lightSide);
        }

        private void AppendFirstSegmentToActiveSegment()
        {
            foreach (PointProcessingContext bufferedContext in _firstSegmentBuffer)
            {
                _activeSegment.Add(bufferedContext);
            }            
        }

        private bool IsLastPoint(CPUHullPart hull, ref PointProcessingContext context)
        {
            return context.Index >= hull.Inner.TransformedHullVertices.Length - 1;
        }

        private PointType GetPointType(ref PointProcessingContext context)
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
                return PointType.LeftEdge;
            if (dot1 >= 0 && dot2 < 0)
                return PointType.RightEdge;
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
