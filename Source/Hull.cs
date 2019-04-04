using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Penumbra.Geometry;
using Penumbra.Graphics;
using Penumbra.Utilities;
using Polygon = Penumbra.Utilities.FastList<Microsoft.Xna.Framework.Vector2>;
using Indices = Penumbra.Utilities.FastList<int>;

namespace Penumbra
{
    /// <summary>
    /// A hull is an object from which shadows are cast.
    /// It is a simple convex or concave polygon impassable by light rays.
    /// </summary>
    public class Hull
    {
        private readonly ExtendedObservableCollection<Vector2> _rawLocalPoints =
            new ExtendedObservableCollection<Vector2>();

        private bool _worldDirty = true;
        private bool _pointsDirty = true;

        /// <summary>
        /// Constructs a new instance of <see cref="Hull"/>.
        /// </summary>
        /// <param name="points">
        /// Points of the hull polygon. In order for the hull to be valid, the points must form:
        /// <list type="number">
        /// <item><description>A polygon with at least 3 points.</description></item>
        /// <item><description>A simple polygon (no two edges intersect with each other).</description></item>
        /// </list>
        /// </param>
        public Hull(params Vector2[] points) : this((IEnumerable<Vector2>)points)
        { }

        /// <summary>
        /// Constructs a new instance of <see cref="Hull"/>.
        /// </summary>
        /// <param name="points">
        /// Points of the hull polygon. In order for the hull to be valid, the points must form:
        /// <list type="number">
        /// <item><description>A polygon with at least 3 points.</description></item>
        /// <item><description>A simple polygon (no two edges intersect with each other).</description></item>
        /// </list>
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

            _rawLocalPoints.CollectionChanged += (s, e) => _pointsDirty = true;
        }

        /// <summary>
        /// Points of the hull polygon. In order for the hull to be valid, the points must form:
        /// <list type="number">
        /// <item><description>A polygon with at least 3 points.</description></item>
        /// <item><description>A simple polygon (no two edges intersect with each other).</description></item>
        /// </list>
        /// Points can be defined in either clockwise or counter-clockwise order.
        /// </summary>
        public IList<Vector2> Points => _rawLocalPoints;

        /// <summary>
        /// Gets or sets if the hull is enabled and participates in shadow casting.
        /// Shadows are only cast from enabled hulls.
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
        /// Gets or sets the origin ((0, 0) point) of the hull's local space.
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
        /// Gets or sets the scale (width and height) along X and Y axes.
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
            if (_pointsDirty)
                UpdatePoints();

            if (_worldDirty)
                UpdateWorld();
        }

        private void UpdatePoints()
        {
            ValidateRawLocalPoints();
            if (Valid)
            {
                ConvertRawLocalPointsToLocalPoints();

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
                    Triangulator.Process(LocalPoints, Indices);
                }

                _worldDirty = true;
            }
            _pointsDirty = false;
        }

        private void UpdateWorld()
        {
            // Calculate local to world transform.
            Calculate.Transform(ref _position, ref _origin, ref _scale, _rotation, out LocalToWorld);

            // Calculate points in world space.
            WorldPoints.Clear();
            int pointCount = LocalPoints.Count;
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 originalPos = LocalPoints[i];
                Vector2.Transform(ref originalPos, ref LocalToWorld, out Vector2 transformedPos);
                WorldPoints.Add(transformedPos);
            }

            // Calculate bounds.
            WorldPoints.GetBounds(out Bounds);

            Dirty = true;
            _worldDirty = false;
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

        /// <summary>
        /// Factory method for creating a rectangular <see cref="Hull"/> with points defined so that
        /// min vertex is at (0.0, 0.0) and max vertex is at (1.0, 1.0).
        /// </summary>
        /// <param name="position">Optional initial position. Default is (0.0, 0.0).</param>
        /// <param name="scale">Optional initial scale. Default is (1.0, 1.0).</param>
        /// <param name="rotation">Optional initial rotation in radians. Default is 0.0.</param>
        /// <param name="origin">Optional initial origin. Default is (0.5, 0.5).</param>
        /// <returns>A rectangular <see cref="Hull"/>.</returns>
        public static Hull CreateRectangle(Vector2? position = null, Vector2? scale = null, float rotation = 0.0f, Vector2? origin = null) =>
            new Hull(new Vector2(1.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f), new Vector2(1.0f, 0.0f))
            {
                Position = position ?? Vector2.Zero,
                Origin = origin ?? new Vector2(0.5f),
                Scale = scale ?? new Vector2(1.0f),
                Rotation = rotation
            };
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
                // If hull is not ignored by light, valid and enabled:
                // 1. test AABB intersection
                // 2. test point is contained in polygon
                if (!light.IgnoredHulls.Contains(hull) &&
                    hull.Enabled &&
                    hull.Valid &&
                    light.Bounds.Intersects(ref hull.Bounds) &&
                    hull.WorldPoints.Contains(ref light._position))
                    return true;
            }
            return false;
        }
    }
}

