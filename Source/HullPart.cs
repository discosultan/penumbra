using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Triangulation;

namespace Penumbra
{
    public class HullPart // TODO: internal
    {
        private readonly Hull _hull;

        private bool _transformedHullVerticesDirty = true;
        private Vector2[] _transformedHullVertices;

        private bool _radiusDirty = true;
        private bool _centroidDirty = true;

        private float _radius;
        private Vector2 _centroid;

        // clockwise winding order
        public HullPart(Hull hull, Vector2[] points)
        {
            _hull = hull;
            CalculatePointsAndIndices(points);
        }

        public Hull Component => _hull;

        public Vector2[] Points { get; private set; }
        public int[] Indices { get; private set; }
        public bool Enabled { get; set; }

        public IEnumerable<Vector2> TransformedPoints
        {
            get { return TransformedHullVertices; }
        }

        public Vector2[] TransformedHullVertices
        {
            get
            {
                if (_transformedHullVerticesDirty)
                {
                    if (_transformedHullVertices == null)
                        _transformedHullVertices = new Vector2[Points.Length];

                    Matrix transform = _hull.WorldTransform;
                    for (int i = 0; i < Points.Length; i++)
                    {
                        Vector2 originalPos = Points[i];
                        Vector2 transformedPos;
                        Vector2.Transform(ref originalPos, ref transform, out transformedPos);
                        _transformedHullVertices[i] = transformedPos;
                    }
                    _transformedHullVerticesDirty = false;
                }
                return _transformedHullVertices;
            }
        }

        public Vector2 Centroid
        {
            get
            {
                if (_centroidDirty)
                {
                    _centroid = TransformedHullVertices.CalculateCentroid();
                    _centroidDirty = false;
                }
                return _centroid;
            }
        }

        public float Radius
        {
            get
            {
                if (_radiusDirty)
                {
                    _radius = TransformedHullVertices.Max(x => Vector2.Distance(x, Centroid));
                    _radiusDirty = false;
                }
                return _radius;
            }
        }

        public void SetRadiusDirty()
        {
            _radiusDirty = true;
        }

        public void SetDirty()
        {
            _transformedHullVerticesDirty = true;
            _centroidDirty = true;
        }

        private void CalculatePointsAndIndices(Vector2[] points)
        {
            Vector2[] outputPoints;
            int[] outputIndices;
            Triangulator.Triangulate(
                points,
                WindingOrder.CounterClockwise,
                WindingOrder.Clockwise,
                WindingOrder.Clockwise,
                out outputPoints,
                out outputIndices);
            Points = outputPoints;
            Indices = outputIndices;
        }        
    }
}
