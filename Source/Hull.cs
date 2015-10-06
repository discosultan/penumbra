using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Penumbra.Geometry;
using Penumbra.Graphics;
using Penumbra.Utilities;
using Polygon = Penumbra.Utilities.FastList<Microsoft.Xna.Framework.Vector2>;
using Indices = Penumbra.Utilities.FastList<int>;

namespace Penumbra
{
    /// <summary>
    /// Represents a shadow hull in the scene. A simple convex or concave polygon impassable by light
    /// from which shadows are cast.
    /// </summary>
    public sealed class Hull
    {                                                        
        private readonly ExtendedObservableCollection<Vector2> _rawLocalPoints = 
            new ExtendedObservableCollection<Vector2>();        
        // Used by the triangulator in order not to allocate on the heap.
        private readonly Indices _intermediaryIndicesBuffer = new Indices();

        private bool _worldDirty = true;
        private bool _pointsDirty = true;

        /// <summary>
        /// Constructs a new instance of <see cref="Hull"/>.
        /// </summary>
        /// <param name="points">
        /// Points of the hull polygon. In order for the hull to be valid, the points must form:
        /// 1. A polygon with atleast 3 points.
        /// 2. A simple polygon (no two edges intersect with each other).
        /// Points can be defined in either clockwise or counter-clockwise order.
        /// </param>
        public Hull(IEnumerable<Vector2> points = null)            
        {
            if (points != null)
            {
                _rawLocalPoints.AddRange(points);
                ValidateRawLocalPoints();
                if (Valid)
                    ConvertRawLocalPointsToLocalPoints();
            }

            _rawLocalPoints.CollectionChanged += (s, e) =>
            {
                ValidateRawLocalPoints();
                if (Valid)
                {
                    ConvertRawLocalPointsToLocalPoints();
                    if (e.Action == NotifyCollectionChangedAction.Add)
                        foreach (Vector2 point in e.NewItems)
                            Logger.Write($"New point at {point}.");                    
                    _worldDirty = true;
                    _pointsDirty = true;
                }                
            };            
        }

        /// <summary>
        /// Points of the hull polygon. In order for the hull to be valid, the points must form:
        /// 1. A polygon with atleast 3 points.
        /// 2. A simple polygon (no two edges intersect with each other).
        /// Points can be defined in either clockwise or counter-clockwise order.
        /// </summary>
        public IList<Vector2> Points => _rawLocalPoints;

        /// <summary>
        /// Gets or sets if the hull is enabled and participates in shadow casting.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets if the hull forms a valid polygon and participates in shadow casting. See
        /// Points property for rules of a valid polygon.
        /// </summary>
        public bool Valid { get; private set; }        

        private Vector2 _position;
        /// <summary>
        /// Gets or sets the position of the hull in world space.
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {                    
                    _position = value;
                    _worldDirty = true;
                }
            }
        }
        
        private Vector2 _origin;
        /// <summary>
        /// Gets or sets the origin ([0;0] point) of the hull's local space.
        /// </summary>
        public Vector2 Origin
        {
            get { return _origin; }
            set
            {
                if (_origin != value)
                {                    
                    _origin = value;
                    _worldDirty = true;
                }
            }
        }
        
        private float _rotation;
        /// <summary>
        /// Gets or sets the rotation of the hull in radians.
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                if (_rotation != value)
                {                    
                    _rotation = value;
                    _worldDirty = true;
                }
            }
        }
        
        private Vector2 _scale = Vector2.One;
        /// <summary>
        /// Gets or sets the scale along X and Y axes.
        /// </summary>
        public Vector2 Scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {                    
                    _scale = value;
                    _worldDirty = true;
                }
            }
        }        

        internal bool Dirty;

        internal BoundingRectangle Bounds;

        internal Matrix LocalToWorld;

        internal Polygon LocalPoints { get; } = new Polygon();

        internal Polygon WorldPoints { get; } = new Polygon();

        internal Indices Indices { get; } = new Indices();

        internal bool IsConvex;                        

        internal void Update()
        {
            if (_worldDirty)
            {
                UpdatePoints();

                // Calculate local to world transform.                
                Calc.CreateTransform(ref _position, ref _origin, ref _scale, _rotation, out LocalToWorld);

                // Calculate points in world space.
                WorldPoints.Clear();                
                int pointCount = LocalPoints.Count;
                for (int i = 0; i < pointCount; i++)
                {
                    Vector2 originalPos = LocalPoints[i];
                    Vector2 transformedPos;
                    Vector2.Transform(ref originalPos, ref LocalToWorld, out transformedPos);                    
                    WorldPoints.Add(transformedPos);
                }

                // Calculate bounds.
                WorldPoints.GetBounds(out Bounds);

                _worldDirty = false;
                Dirty = true;
            }
        }

        private void UpdatePoints()
        {
            if (_pointsDirty)
            {
                IsConvex = LocalPoints.IsConvex();
                Indices.Clear();                

                if (IsConvex)
                {
                    int numTriangles = LocalPoints.Count - 2;
                    for (int i = numTriangles - 1; i >= 0; i--)
                    {
                        Indices.Add(0);
                        Indices.Add(i + 2);
                        Indices.Add(i + 1);
                    }
                }
                else
                {
                    _intermediaryIndicesBuffer.Clear();
                    Triangulator.Process(LocalPoints, _intermediaryIndicesBuffer, Indices);
                }

                _pointsDirty = false;                
            }
        }

        // Raw local points are points unmodified by the system. Local points are always in CCW order.
        private void ConvertRawLocalPointsToLocalPoints()
        {
            LocalPoints.Clear();
            LocalPoints.AddRange(_rawLocalPoints);
            if (!LocalPoints.IsCounterClockWise())
                LocalPoints.ReverseWindingOrder();            
        }

        private void ValidateRawLocalPoints()
        {            
            if (_rawLocalPoints.Count < 3)
            {
                Valid = false;
                Logger.Write("Hull invalid: polygon point count less than 3.");
            }
            else if (!_rawLocalPoints.IsSimple())
            {
                Valid = false;
                Logger.Write("Hull invalid: complex polygon is not supported.");
            }
            else
            {
                Valid = true;
                Logger.Write("Hull valid.");
            }
        }
    }

    internal class HullList : ObservableCollection<Hull>
    {
        public bool Dirty { get; set; }

        public void Update()
        {
            int hullCount = Count;
            for (int i = 0; i < hullCount; i++)
            {
                Hull hull = this[i];

                hull.Update();                
                Dirty = Dirty || hull.Dirty;
                hull.Dirty = false;
            }
        }

        public bool Contains(Light light)
        {
            int hullCount = Count;
            for (int i = 0; i < hullCount; i++)
            {
                Hull hull = this[i];
                // If hull is valid and enabled:
                // 1. test AABB intersection
                // 2. test point is contained in polygon
                if (hull.Enabled && hull.Valid && light.Bounds.Intersects(ref hull.Bounds) && hull.WorldPoints.Contains(ref light._position))
                    return true;
            }
            return false;
        }
    }
}

