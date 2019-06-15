using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics.Providers;
using Penumbra.Graphics.Renderers;
using Penumbra.Utilities;

namespace Penumbra
{
    /// <summary>
    /// A light is an object which lights the world and casts shadows from <see cref="Hull"/>s.
    /// </summary>
    /// <remarks>
    /// It is an abstract class - one of the three concrete implementations should be used instead:
    /// <see cref="PointLight" />, <see cref="Spotlight" />, <see cref="TexturedLight" />.
    /// </remarks>
    public abstract class Light
    {
        private const float Epsilon = 1e-5f;

        private readonly ObservableCollection<Hull> _ignoredHulls = new ObservableCollection<Hull>();
        // Used privately to determine when to calculate world transform and bounds.
        private bool _worldDirty = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Light"/> class.
        /// </summary>
        protected Light()
        {
            _ignoredHulls.CollectionChanged += (s, e) => Dirty = true;
        }

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

        private Vector2 _origin = new Vector2(0.5f);
        /// <summary>
        /// Gets or sets the origin (anchor) of the light. It is used for both positioning and
        /// rotating. Normalized to the range [0..1].
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each light is essentially a quad. Origin is the anchor point which marks the (0, 0) point on that quad (in local space).
        /// Depending if you are operating in SpriteBatch's screen space (y-axis runs from top to bottom) origin (0, 0)
        /// represents the light quad's top left corner while (1, 1) represents the bottom right corner. The reason it's normalized to [0..1]
        /// is so that if you change the scale of the light, you wouldn't need to change the origin: an origin (0.5, 0.5) would still mark
        /// the center of the light.
        /// </para>
        /// <para>
        /// When it comes to the setter, there is no automatic normalization being done: it is expected to be set in its normalized form.
        /// The reason values outside [0..1] range are allowed is that it might be desirable for some weird rotation scenarios,
        /// though such usage should be rather uncommon.
        /// </para>
        /// <para>
        /// Default value is usually sufficient for <see cref="PointLight"/> and <see cref="Spotlight"/>.
        /// </para>
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

        private Vector2 _scale = new Vector2(100.0f);
        /// <summary>
        /// Gets or sets the scale (width and height) along X and Y axes.
        /// </summary>
        /// <remarks>
        /// Not to be confused with <see cref="Radius"/>, scale determines the attenuation
        /// of the light or how far the light rays reach (range of the light), while radius
        /// determines the radius of the light source (the area where light is emitted).
        /// <see cref="Radius"/> for more info.
        /// </remarks>
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

        private Color _nonPremultipliedColor = Color.White;
        private Vector3 _color = Vector3.One;
        /// <summary>
        /// Gets or sets the color of the light. Color is in non-premultiplied format.
        /// Default is white.
        /// </summary>
        public Color Color
        {
            get { return _nonPremultipliedColor; }
            set
            {
                _nonPremultipliedColor = value;
                Calculate.FromNonPremultiplied(value, out _color);
            }
        }

        private float _intensity = 1.0f;
        /// <summary>
        /// Gets or sets the intensity of the color applied to the final scene.
        /// Color will be raised to the power of intensity.
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
        internal float IntensityFactor { get; private set; } = 1.0f;

        private float _radius = 20.0f;
        /// <summary>
        /// Gets or sets the radius of the light source (the area where light is emitted).
        /// This determines the shape of the umbra and penumbra regions for cast shadows.
        /// </summary>
        /// <remarks>
        /// Not to be confused with <see cref="Scale"/>, while radius is only used to control
        /// the softness of the shadow being cast and should usually be kept at a small value,
        /// scale is used to determine how far the light rays reach (range of the light).
        /// <see cref="Scale"/> for more info.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets how the shadow <see cref="Hull"/>s are shadowed. See
        /// <see cref="ShadowType"/> for more information.
        /// </summary>
        public ShadowType ShadowType { get; set; } = ShadowType.Illuminated;

        /// <summary>
        /// Gets a list of hulls not participating in the light's shadow casting process.
        /// </summary>
        public IList<Hull> IgnoredHulls => _ignoredHulls;

        // Cleared by the engine. Used by other systems to know when to regenerate shadows for the light.
        internal bool Dirty;

        internal BoundingRectangle Bounds;

        internal Matrix LocalToWorld;

        internal virtual EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            renderer._fxLightParamColor.SetValue(_color);
            renderer._fxLightParamIntensity.SetValue(IntensityFactor);
            return null;
        }

        internal void Update()
        {
            if (_worldDirty)
            {
                // Calculate local to world.
                Calculate.Transform(ref _position, ref _origin, ref _scale, _rotation, out LocalToWorld);
                CalculateBounds();

                _worldDirty = false;
                Dirty = true;
            }
        }

        internal bool Intersects(CameraProvider camera) => Bounds.Intersects(ref camera.Bounds);

        internal bool Intersects(Hull hull) => Bounds.Intersects(ref hull.Bounds);

        private void CalculateBounds()
        {
            if (_rotation == 0.0f)
            {
                Vector2.Multiply(ref _origin, ref _scale, out Vector2 min);
                Vector2.Subtract(ref _position, ref min, out min);

                Vector2.Add(ref min, ref _scale, out Vector2 max);

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
        /// <summary>
        /// Occluded shadow hulls (hulls behind other hulls) are not lit.
        /// </summary>
        Occluded
    }
}