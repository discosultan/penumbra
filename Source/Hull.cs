using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Penumbra.Geometry;
using Penumbra.Graphics;
using Penumbra.Utilities;
using Polygon = Penumbra.Utilities.FastList<Microsoft.Xna.Framework.Vector2>;
using Indices = Penumbra.Utilities.FastList<int>;

namespace Penumbra
{
    public class Hull
    {
        private bool _localToWorldDirty = true;
        private Matrix _localToWorld;

        private bool _isConvex;
        private bool _enabled = true;
        private Vector2 _position;
        private Vector2 _origin;
        private float _rotation;
        private Vector2 _scale = Vector2.One;

        private bool _worldPointsDirty = true;
        private readonly ExtendedObservableCollection<Vector2> _rawLocalPoints = 
            new ExtendedObservableCollection<Vector2>(); 
        private readonly Polygon _worldPoints = new Polygon();
        private readonly Indices _indices = new Indices();
        private readonly Indices _intermediaryIndicesBuffer = new Indices();

        private bool _indicesDirty = true;
        private bool _radiusDirty = true;
        private bool _centroidDirty = true;
        private bool _convexDirty = true;

        private float _radius;
        private Vector2 _centroid;

        #region Constructors

        public Hull(IEnumerable<Vector2> points = null)            
        {
            _rawLocalPoints.CollectionChanged += (s, e) =>
            {
                ValidateRawLocalPoints();
                if (Valid)
                {
                    ConvertRawLocalPoints();
                    SetDirty();
                    if (e.Action == NotifyCollectionChangedAction.Add)
                        foreach (Vector2 point in e.NewItems)
                            Logger.Write($"New point at {point}.");
                }
            };

            if (points != null)
                _rawLocalPoints.AddRange(points);            

            //Check.True(LocalPoints.IsSimple(), "Input points must form a simple polygon, meaning that no two edges may intersect with each other.");
        }   

        #endregion        

        #region Public Properties                

        public IList<Vector2> Points => _rawLocalPoints;

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
                    var cos = (float)Math.Cos(Rotation);
                    var sin = (float)Math.Sin(Rotation);

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

        internal Polygon LocalPoints { get; } = new Polygon();

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

        internal Indices Indices
        {
            get
            {
                if (_indicesDirty)
                {
                    _indices.Clear();
                    if (IsConvex)
                    {
                        int numTriangles = LocalPoints.Count - 2;
                        for (int i = numTriangles - 1; i >= 0; i--)
                        {
                            _indices.Add(0);
                            _indices.Add(i + 2);
                            _indices.Add(i + 1);
                        }
                    }
                    else
                    {
                        Triangulator.Process(LocalPoints, _intermediaryIndicesBuffer, _indices);
                    }
                    _indicesDirty = false;
                }
                return _indices;
            }            
        }


        internal bool IsConvex
        {
            get
            {
                if (_convexDirty)
                {
                    _isConvex = LocalPoints.IsConvex();
                    _convexDirty = false;
                }
                return _isConvex;
            }
        }

        internal BoundingRectangle Bounds
        {
            get { return WorldPoints.GetCollisionBox(); } // TODO: cache
        }

        internal HullComponentDirtyFlags DirtyFlags { get; set; } = HullComponentDirtyFlags.All;        

        private void SetDirty()
        {
            _indicesDirty = true;
            _convexDirty = true;
            _centroidDirty = true;
            _localToWorldDirty = true;
            _radiusDirty = true;
            _worldPointsDirty = true;
            DirtyFlags = HullComponentDirtyFlags.All;
        }

        private void ConvertRawLocalPoints()
        {
            LocalPoints.Clear();
            LocalPoints.AddRange(_rawLocalPoints);
            if (!LocalPoints.IsCounterClockWise())
                LocalPoints.ReverseWindingOrder();            
        }

        private void ValidateRawLocalPoints()
        {
            Valid = true;            

            if (_rawLocalPoints.Count < 3)  
                Valid = false;                            
            else if (!_rawLocalPoints.IsSimple())            
                Valid = false;

            Logger.Write($"Polygon valid:{Valid}");
        }
    }

    internal static class HullExtensions
    {
        public static void ClearDirtyFlags(this IList<Hull> hulls)
        {
            int hullCount = hulls.Count;
            for (int i = 0; i < hullCount; i++)
                hulls[i].DirtyFlags &= 0;
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

