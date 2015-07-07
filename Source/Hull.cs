using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Triangulation;
using Penumbra.Utilities;
//using Polygon = System.Collections.Generic.List<Microsoft.Xna.Framework.Vector2>;

namespace Penumbra
{
    public class Hull
    {                
        private bool _worldTransformDirty = true;        
        private Matrix _worldTransform;

        private bool _enabled;
        private Vector2 _position;
        private Vector2 _origin;
        private float _rotation;
        private Vector2 _scale;

        private HullPart[] _parts;

        internal event EventHandler SetDirty;

        public Hull(IList<Vector2> points, WindingOrder windingOrder = WindingOrder.Clockwise)
        {
            Check.ArgumentNotLessThan(points.Count, 3, "points", "Hull must consist minimum of 3 points.");

            CalculateParts(points, windingOrder);

            // Set default values.
            Scale = Vector2.One;
            DirtyFlags = HullComponentDirtyFlags.All;
            Enabled = true;            
        }

        internal HullPart[] Parts => _parts;        

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    DirtyFlags |= HullComponentDirtyFlags.Enabled;                    
                    _enabled = value;
                    foreach (HullPart part in _parts)                    
                        part.Enabled = value;                    
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
                    SetAndRaiseDirty();
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
                    SetAndRaiseDirty();
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
                    SetAndRaiseDirty();
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
                    SetAndRaiseDirty();
                    foreach (HullPart part in _parts)
                        part.SetRadiusDirty();                    
                    _scale = value;
                }
            }
        }        

        internal bool AnyDirty(HullComponentDirtyFlags flags)
        {
            return (DirtyFlags & flags) != 0;
        }                

        internal Matrix WorldTransform
        {
            get
            {
                if (_worldTransformDirty)
                {
                    _worldTransform = Matrix.Identity;

                    // Create the matrices
                    float cos = Calc.Cos(Rotation);
                    float sin = Calc.Sin(Rotation);

                    // vertexMatrix = scale * rotation * translation;
                    _worldTransform.M11 = _scale.X * cos;
                    _worldTransform.M12 = _scale.X * sin;
                    _worldTransform.M21 = _scale.Y * -sin;
                    _worldTransform.M22 = _scale.Y * cos;
                    _worldTransform.M41 = _position.X - _origin.X;
                    _worldTransform.M42 = _position.Y - _origin.Y;

                    _worldTransformDirty = false;
                }
                return _worldTransform;
            }
        }

        internal HullComponentDirtyFlags DirtyFlags { get; set; }        

        private void SetAndRaiseDirty()
        {
            _worldTransformDirty = true;            
            foreach (HullPart part in _parts)
            {
                part.SetDirty();
            }
            SetDirty?.Invoke(this, EventArgs.Empty);
        }

        private void CalculateParts(IList<Vector2> points, WindingOrder windingOrder)
        {
            Polygon polygon = new Polygon(points.ToList(), windingOrder);

            // Ensure clockwise winding order.
            polygon.EnsureWindingOrder(WindingOrder.CounterClockwise);

            if (polygon.IsConvex())
            {
                _parts = new[] { new HullPart(this, polygon) };
            }
            else
            {
                var polygons = Polygon.DecomposeIntoConvex(polygon);                

                _parts = new HullPart[polygons.Count];
                for (int i = 0; i < polygons.Count; i++)
                {
                    _parts[i] = new HullPart(this, polygons[i]);
                }
            }
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

