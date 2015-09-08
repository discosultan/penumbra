using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics.Providers;
using Penumbra.Utilities;

namespace Penumbra.Core
{
    public class Light
    {
        internal Vector2 _position;

        private bool _castsShadows;
        private bool _enabled;        
        private float _range;
        private float _radius;

        private bool _worldToLocalDirty = true;
        private Matrix _worldToLocal;

        public Light(Texture texture = null) // TODO: allow creation of light without tex, use cached tex based on radius internally in these cases.
        {
            Enabled = true;
            CastsShadows = true;
            Range = 100f;
            Radius = 20f;
            Intensity = 1;
            ShadowType = ShadowType.Illuminated;
            Color = new Color(1f,1f,1f,1f);
            Texture = texture;
            DirtyFlags = LightComponentDirtyFlags.All;
        }        

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    DirtyFlags |= LightComponentDirtyFlags.Enabled;
                    _enabled = value;
                }
            }
        }

        public bool CastsShadows
        {
            get { return _castsShadows; }
            set
            {
                if (_castsShadows != value)
                {
                    DirtyFlags |= LightComponentDirtyFlags.CastsShadows;
                    _castsShadows = value;
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
                    DirtyFlags |= LightComponentDirtyFlags.Position;
                    _position = value;
                    _worldToLocalDirty = true;
                }
            }
        }

        public float Range
        {
            get { return _range; }
            set
            {
                Check.ArgumentNotLessThan(value, 1, "value", "Range cannot be smaller than 1.");
                if (_range != value)
                {
                    DirtyFlags |= LightComponentDirtyFlags.Range;
                    _range = value;
                    _worldToLocalDirty = true;
                }
            }
        }

        public float RangeSquared => Range*Range;

        public float Radius
        {
            get { return _radius; }
            set
            {
                Check.ArgumentWithinRange(value, 1, Range, "value", "Radius cannot cannot be smaller than 1 and larger than Range.");
                if (_radius != value)
                {
                    DirtyFlags |= LightComponentDirtyFlags.Radius;
                    _radius = value;
                }
            }
        }

        public float Intensity { get; set; }
        private ShadowType _shadowType;
        public ShadowType ShadowType
        {
            get { return _shadowType; }
            set
            {
                if (value != _shadowType)
                {
                    _shadowType = value;
                    DirtyFlags |= LightComponentDirtyFlags.ShadowType;                    
                }
            }
        }
        public Color Color { get; set; }
        public Texture Texture { get; set; }

        public Matrix LocalToWorld
        {
            get
            {
                var transform = Matrix.Identity;
                // Scaling.
                transform.M11 = Range;
                transform.M22 = Range;
                // Translation.
                transform.M41 = Position.X;
                transform.M42 = Position.Y;
                return transform;
            }
        }

        public Matrix WorldToLocal
        {
            get
            {
                if (_worldToLocalDirty)
                {
                    _worldToLocal = Matrix.Invert(LocalToWorld);
                    _worldToLocalDirty = false;
                }
                return _worldToLocal;
            }   
        }


        //internal float IntensityFactor => 1 / (Intensity * Intensity);
        internal float IntensityFactor => 1 / Intensity;
        internal LightComponentDirtyFlags DirtyFlags { get; set; }

        internal bool AnyDirty(LightComponentDirtyFlags flags)
        {
            return (DirtyFlags & flags) != 0;
        }

        internal BoundingRectangle Bounds
        {
            get
            {
                Vector2 min = Position - new Vector2(Range);
                Vector2 max = Position + new Vector2(Range);
                return new BoundingRectangle(min, max);
            }
        }

        internal bool Intersects(CameraProvider camera)
        {
            return Bounds.Intersects(ref camera.Bounds);
        }

        internal bool Intersects(Hull Hull)
        {
            // Ref: Jason Gregory Game Engine Architecture 2nd p.172
            float sumOfRadiuses = Range + Hull.Radius;
            return Vector2.DistanceSquared(Position, Hull.Centroid) < sumOfRadiuses * sumOfRadiuses;
        }        

        internal bool ContainedIn(IList<Hull> hulls)
        {
            int hullCount = hulls.Count;
            for (int i = 0; i < hullCount; i++)
            {
                Hull hull = hulls[i];
                // If hull is valid and enabled:
                // 1. test AABB intersection
                // 2. test point is contained in polygon
                if (hull.Enabled && hull.Valid && Bounds.Intersects(hull.Bounds) && hull.WorldPoints.Contains(ref _position))
                    return true;
            }
            return false;
        }
    }

    public enum ShadowType
    {
        Illuminated,
        Solid,        
        //Occluded
    }

    [Flags]
    internal enum LightComponentDirtyFlags
    {
        CastsShadows = 1 << 0,
        Position = 1 << 1,
        Radius = 1 << 2,
        Range = 1 << 3,
        Enabled = 1 << 4,
        ShadowType = 1 << 5,
        All = int.MaxValue
    }
}
