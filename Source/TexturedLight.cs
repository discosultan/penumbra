using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    public sealed class TexturedLight : Light
    {
        public TexturedLight(Texture2D texture)
        {            
            if (texture != null)
            {
                Texture = texture;
                Width = texture.Width;
                Height = texture.Height;
            }
        }

        private Vector2 _origin;
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

        /// <summary>
        /// Gets or sets the texture used to determine in what shape to render the light.
        /// A spotlight could be simulated with a spotlight texture. If no texture is set,
        /// uses a linear falloff equation to render a point light shaped light. 
        /// </summary>
        public Texture2D Texture { get; set; }
        
        public Matrix TextureTransform { get; set; } = Matrix.Identity;

        private float _width = 200.0f;
        public float Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _worldDirty = true;
                    _width = value;
                }
            }
        }

        private float _height = 200.0f;
        public float Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _worldDirty = true;
                    _height = value;
                }
            }
        }

        internal override void CalculateLocalToWorld(out Matrix transform)
        {
            Vector2 centerPosition;
            Vector2.Subtract(ref _position, ref _origin, out centerPosition);
                        
            transform = Matrix.Identity;
            // Scaling.
            transform.M11 = Width * 0.5f;
            transform.M22 = Height * 0.5f;
            // Translation.
            transform.M41 = centerPosition.X;
            transform.M42 = centerPosition.Y;
        }

        internal override void CalculateBounds(out BoundingRectangle bounds)
        {
            var halfSize = new Vector2(Width, Height) * 0.5f;

            var min = Position - Origin - halfSize;
            var max = Position - Origin + halfSize;

            bounds = new BoundingRectangle(min, max);
        }

        internal override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);

            renderer._fxLightParamTexture.SetValue(Texture);
            renderer._fxLightParamTextureTransform.SetValue(TextureTransform);

            return renderer._fxTexturedLightTech;
        }
    }
}
