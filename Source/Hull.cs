using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Utilities;
//using Indices = System.Collections.Generic.List<int>;
using Indices = Penumbra.Utilities.FastList<int>;

namespace Penumbra
{
    public class Hull
    {
        private bool _localToWorldDirty = true;
        private Matrix _localToWorld;

        private bool _enabled = true;
        private Vector2 _position;
        private Vector2 _origin;
        private float _rotation;
        private Vector2 _scale = Vector2.One;

        private readonly FastList<PointNormals> _localNormals = new FastList<PointNormals>();
        private readonly FastList<PointNormals> _worldNormals = new FastList<PointNormals>();

        private bool _worldNormalsDirty = true;
        private bool _worldPointsDirty = true;
        private readonly Polygon _worldPoints = new Polygon();
        private readonly Indices _indices = new Indices();

        private bool _indicesDirty = true;
        private bool _localNormalsDirty = true;
        private bool _radiusDirty = true;
        private bool _centroidDirty = true;

        private float _radius;
        private Vector2 _centroid;

        #region Constructors

        public Hull(ICollection<Vector2> points)
        {
            Check.ArgumentNotNull(points, nameof(points), "Points cannot be null.");
            Check.ArgumentNotLessThan(points.Count, 3, "points", "Hull must consist minimum of 3 points.");

            LocalPoints = new Polygon(points);

            Check.True(LocalPoints.IsSimple(), "Input points must form a simple polygon, meaning that no two edges may intersect with each other.");
            
            //LocalPoints.EnsureCounterClockwiseWindingOrder();
        }

        public Hull()
        {
            LocalPoints = new Polygon();
        }

        #endregion        

        #region Public Properties

        public int Count => LocalPoints.Count;

        // TODO: Do we want this?
        public int Layer { get; set; }

        public bool Valid { get; private set; }        

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    DirtyFlags |= HullComponentDirtyFlags.Enabled;
                    _enabled = value;
                }
            }
        }

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    DirtyFlags |= HullComponentDirtyFlags.Position;
                    SetDirty();
                    _position = value;
                }
            }
        }

        public Vector2 Origin
        {
            get { return _origin; }
            set
            {
                if (_origin != value)
                {
                    DirtyFlags |= HullComponentDirtyFlags.Origin;
                    SetDirty();
                    _origin = value;
                }
            }
        }

        public float Rotation
        {
            get { return _rotation; }
            set
            {
                if (_rotation != value)
                {
                    DirtyFlags |= HullComponentDirtyFlags.Rotation;
                    SetDirty();
                    _rotation = value;
                }
            }
        }

        public Vector2 Scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {
                    DirtyFlags |= HullComponentDirtyFlags.Scale;
                    SetDirty();                    
                    _scale = value;
                }
            }
        }

        #endregion

        #region Public Methods

        public void Add(Vector2 point)
        {
            LocalPoints.Add(point);
            Validate();
            //if (Valid)
            //    LocalPoints.EnsureCounterClockwiseWindingOrder();
            SetDirty();            
        }

        public void RemoveAt(int index)
        {
            LocalPoints.RemoveAt(index);
            SetDirty();
            Validate();
        }

        #endregion

        internal bool AnyDirty(HullComponentDirtyFlags flags)
        {
            return (DirtyFlags & flags) != 0;
        }

        internal Vector2 Centroid
        {
            get
            {
                if (_centroidDirty)
                {
                    _centroid = WorldPoints.GetCentroid();
                    _centroidDirty = false;
                }
                return _centroid;
            }
        }

        internal float Radius
        {
            get
            {
                if (_radiusDirty)
                {
                    _radius = WorldPoints.GetRadius();
                    _radiusDirty = false;
                }
                return _radius;
            }
        }

        internal Matrix LocalToWorld
        {
            get
            {
                if (_localToWorldDirty)
                {
                    _localToWorld = Matrix.Identity;

                    // Create the matrices
                    float cos = Calc.Cos(Rotation);
                    float sin = Calc.Sin(Rotation);

                    // vertexMatrix = scale * rotation * translation;
                    _localToWorld.M11 = _scale.X * cos;
                    _localToWorld.M12 = _scale.X * sin;
                    _localToWorld.M21 = _scale.Y * -sin;
                    _localToWorld.M22 = _scale.Y * cos;
                    _localToWorld.M41 = _position.X - _origin.X;
                    _localToWorld.M42 = _position.Y - _origin.Y;

                    _localToWorldDirty = false;
                }
                return _localToWorld;
            }
        }


        internal Polygon LocalPoints { get; }        

        internal Polygon WorldPoints
        {
            get
            {
                if (_worldPointsDirty)
                {
                    _worldPoints.Clear(true);

                    Matrix transform = LocalToWorld;
                    for (int i = 0; i < LocalPoints.Count; i++)
                    {
                        Vector2 originalPos = LocalPoints[i];
                        Vector2 transformedPos;
                        Vector2.Transform(ref originalPos, ref transform, out transformedPos);
                        //_transformedHullVertices[i] = transformedPos;
                        _worldPoints.Add(transformedPos);
                    }
                    _worldPointsDirty = false;
                }
                return _worldPoints;
            }
        }

        internal FastList<PointNormals> LocalNormals
        {
            get
            {
                if (_localNormalsDirty)
                {
                    _localNormals.Clear(true);
                    for (int i = 0; i < LocalPoints.Count; i++)
                    {
                        Vector2 currentPos = LocalPoints[i];
                        Vector2 prevPos = LocalPoints.PreviousElement(i);
                        Vector2 nextPos = LocalPoints.NextElement(i);

                        Vector2 n1 = VectorUtil.Rotate90CW(currentPos - prevPos);
                        Vector2 n2 = VectorUtil.Rotate90CW(nextPos - currentPos);
                        n1.Normalize();
                        n2.Normalize();

                        //// Ref: http://stackoverflow.com/a/25646126/1466456
                        Vector2 currentToPrev = prevPos - currentPos;
                        Vector2 currentToNext = nextPos - currentPos;
                        // TODO: Find more optimal sln
                        float angle = ((Calc.Atan2(currentToNext.X, currentToNext.Y) - Calc.Atan2(currentToPrev.X, currentToPrev.Y) + Calc.Pi * 2) % (Calc.Pi * 2)) - Calc.Pi;
                        bool isConvex = angle < 0;

                        _localNormals.Add(new PointNormals(ref n1, ref n2, isConvex));                        
                    }
                    _localNormalsDirty = false;
                }
                return _localNormals;
            }
        }

        internal FastList<PointNormals> WorldNormals
        {
            get
            {                
                if (_worldNormalsDirty)
                {
                    _worldNormals.Clear(true);

                    Matrix normalMatrix = Matrix.Identity;

                    float cos = Calc.Cos(Rotation);
                    float sin = Calc.Sin(Rotation);

                    // normalMatrix = scaleInv * rotation;
                    normalMatrix.M11 = (1f / Scale.X) * cos;
                    normalMatrix.M12 = (1f / Scale.X) * sin;
                    normalMatrix.M21 = (1f / Scale.Y) * -sin;
                    normalMatrix.M22 = (1f / Scale.Y) * cos;

                    for (var i = 0; i < LocalNormals.Count; i++)
                    {
                        PointNormals normals = LocalNormals[i];                        
                        PointNormals.Transform(ref normals, ref normalMatrix, out normals);
                        _worldNormals.Add(normals);
                    }

                    _worldNormalsDirty = false;
                }
                return _worldNormals;
            }
        }        

        internal Indices Indices
        {
            get
            {
                if (_indicesDirty)
                {
                    _indices.Clear(true);
                    WorldPoints.GetIndices(WindingOrder.Clockwise, _indices);
                    _indicesDirty = false;
                }
                return _indices;
            }
        }

        internal HullComponentDirtyFlags DirtyFlags { get; set; } = HullComponentDirtyFlags.All;

        private void SetDirty()
        {
            _indicesDirty = true;
            _centroidDirty = true;
            _localToWorldDirty = true;
            _radiusDirty = true;
            _worldPointsDirty = true;
            _worldNormalsDirty = true;
            DirtyFlags = HullComponentDirtyFlags.All;
        }

        private void Validate()
        {
            Valid = true;

            if (LocalPoints.Count < 3)  
                Valid = false;                            
            else if (!LocalPoints.IsSimple())            
                Valid = false;            
        }
    }    

    [Flags]
    internal enum HullComponentDirtyFlags
    {
        Enabled = 1 << 0,
        Position = 1 << 1,
        Rotation = 1 << 2,
        Scale = 1 << 3,
        Origin = 1 << 4,
        All = int.MaxValue
    }
}

