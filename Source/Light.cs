using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics.Providers;
using Penumbra.Utilities;

namespace Penumbra
{
    public class Light
    {        
        public bool Enabled { get; set; } = true;

        public bool CastsShadows { get; set; } = true;

        private Vector2 _position;
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

        private float _range = 100f;
        public float Range
        {
            get { return _range; }
            set
            {                
                if (_range != value)
                {                    
                    _range = value;
                    _worldDirty = true;
                }
            }
        }

        public float Radius { get; set; } = 20.0f;

        public float Intensity { get; set; } = 1.0f;

        public ShadowType ShadowType { get; set; } = ShadowType.Illuminated;

        public Color Color { get; set; } = Color.White;

        public Texture Texture { get; set; }

        public Matrix TextureTransform { get; set; } = Matrix.Identity;

        // Used privately to determine when to calculate world transform and bounds.
        private bool _worldDirty = true;

        // Cleared by the engine. Used by other systems to know if the light's world transform has changed.
        internal bool Dirty;        

        internal BoundingRectangle Bounds;

        internal Matrix LocalToWorld;
        internal Matrix WorldToLocal;

        internal void Update()
        {
            if (_worldDirty)
            {
                // Calculate local to world transform.
                LocalToWorld = Matrix.Identity;
                // Scaling.
                LocalToWorld.M11 = Range;
                LocalToWorld.M22 = Range;
                // Translation.
                LocalToWorld.M41 = Position.X;
                LocalToWorld.M42 = Position.Y;

                // Calculate world to local transform.
                Matrix.Invert(ref LocalToWorld, out WorldToLocal);

                // Calculate bounds.
                Vector2 min = Position - new Vector2(Range);
                Vector2 max = Position + new Vector2(Range);
                Bounds = new BoundingRectangle(min, max);

                _worldDirty = false;
                Dirty = true;
            }
        }

        internal bool Intersects(CameraProvider camera)
        {
            return Bounds.Intersects(ref camera.Bounds);
        }

        internal bool Intersects(Hull Hull)
        {
            return Bounds.Intersects(Hull.Bounds);
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

    /// <summary>
    /// Determines how the shadows cast by the light affect shadow <see cref="Hull"/>s.
    /// </summary>
    public enum ShadowType
    {
        /// <summary>
        /// Shadow hulls are lit by the light.
        /// </summary>
        Illuminated,
        /// <summary>
        /// Shadow hulls are not lit by the light.
        /// </summary>
        Solid,        
        //Occluded
    }
}
