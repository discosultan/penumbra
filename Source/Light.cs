using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics.Providers;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    /// <summary>
    /// A concept of light source casting shadows from shadow <see cref="Hull"/>s.
    /// </summary>
    public abstract class Light
    {
        private const float Epsilon = 1e-5f;

        // Used privately to determine when to calculate world transform and bounds.
        protected bool _worldDirty = true;

        /// <summary>
        /// Gets or sets if the light is enabled and should be rendered.
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Gets or sets if the light casts shadows.
        /// </summary>
        public bool CastsShadows { get; set; } = true;

        internal Vector2 _position;
        /// <summary>
        /// Gets or sets the light's position in world space.
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

        private float _radius = 20.0f;

        /// <summary>
        /// Gets or sets the radius of the light source (the area emitting light). 
        /// This determines the shape of the cast shadow umbra and penumbra regions.
        /// </summary>
        public float Radius
        {
            get { return _radius; }
            set
            {
                value = Math.Max(value, Epsilon);
                if (_radius != value)
                {
                    _radius = value;
                    Dirty = true;
                }
            }
        }
        
        private float _intensity = 1.0f;
        internal float IntensityFactor { get; private set; } = 1.0f;
        /// <summary>
        /// Gets or sets the intensity of the color applied to the final scene.
        /// </summary>
        public float Intensity
        {
            get { return _intensity; }
            set
            {
                _intensity = Math.Max(value, Epsilon);
                IntensityFactor = 1 / _intensity;
            }
        }        
        
        /// <summary>
        /// Gets or sets how the shadow <see cref="Hull"/>s are shadowed. See
        /// <see cref="ShadowType"/> for more information.
        /// </summary>
        public ShadowType ShadowType { get; set; } = ShadowType.Illuminated;
        /// <summary>
        /// Gets or sets the color emitted by the light.
        /// </summary>
        public Color Color { get; set; } = Color.White;        

        // Cleared by the engine. Used by other systems to know if the light's world transform has changed.
        internal bool Dirty;
        
        internal BoundingRectangle Bounds;

        internal Matrix LocalToWorld;              

        internal virtual EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            renderer._fxLightParamColor.SetValue(Color.ToVector3());
            renderer._fxLightParamIntensity.SetValue(IntensityFactor);
            return null; 
        }

        internal void Update()
        {
            if (_worldDirty)
            {
                CalculateLocalToWorld(out LocalToWorld);
                CalculateBounds(out Bounds);

                _worldDirty = false;
                Dirty = true;
            }
        }

        internal abstract void CalculateBounds(out BoundingRectangle bounds);
        internal abstract void CalculateLocalToWorld(out Matrix transform);

        //internal virtual void CalculateBounds(out BoundingRectangle bounds)
        //{
        //    var rangeVector = new Vector2(Range);
        //    Vector2 min = Position - rangeVector;
        //    Vector2 max = Position + rangeVector;
        //    bounds = new BoundingRectangle(min, max);
        //}

        internal bool Intersects(CameraProvider camera)
        {
            return Bounds.Intersects(ref camera.Bounds);
        }

        internal bool Intersects(Hull hull)
        {
            return Bounds.Intersects(ref hull.Bounds);
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
