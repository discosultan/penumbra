using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics.Providers;
using Penumbra.Graphics.Renderers;
using Penumbra.Utilities;

namespace Penumbra
{
    /// <summary>
    /// A concept of light source casting shadows from shadow <see cref="Hull"/>s.
    /// </summary>
    public class Light
    {
        private const float Epsilon = 1e-5f;

        // Used privately to determine when to calculate world transform and bounds.
        private bool _worldDirty = true;

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

        private Vector2 _scale = new Vector2(100.0f);
        /// <summary>
        /// Gets or sets the scale (width and height) of the light.
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
        
        private float _rotation;
        /// <summary>
        /// Gets or sets the rotation of the light in radians.
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
        
        private Vector2 _origin = new Vector2(0.5f);
        /// <summary>
        /// Gets or sets the origin (anchor) of the light. This is used for both positioning and
        /// rotating. Normalized to the range [0..1].        
        /// </summary>
        /// <remarks>
        /// For example, origin (0.5, 0.5) corresponds to the center of the light.
        /// </remarks>
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
                // Calculate local to world.
                Calc.CreateTransform(ref _position, ref _origin, ref _scale, _rotation, out LocalToWorld);
                CalculateBounds();

                _worldDirty = false;
                Dirty = true;
            }
        }     

        internal bool Intersects(CameraProvider camera)
        {
            return Bounds.Intersects(ref camera.Bounds);
        }

        internal bool Intersects(Hull hull)
        {
            return Bounds.Intersects(ref hull.Bounds);
        }

        private void CalculateBounds()
        {
            if (_rotation == 0.0f)
            {
                Vector2 min;
                Vector2.Multiply(ref _origin, ref _scale, out min);
                Vector2.Subtract(ref _position, ref min, out min);

                Vector2 max;                
                Vector2.Add(ref min, ref _scale, out max);

                Bounds = new BoundingRectangle(min, max);
            }
            else
            {                
                var bounds = new BoundingRectangle(Vector2.Zero, Vector2.One);
                BoundingRectangle.Transform(ref bounds, ref LocalToWorld, out Bounds);
            }
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