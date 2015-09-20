using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    public sealed class TexturedLight : Light
    {        
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

        public float Width => Texture.Width;
        public float Height => Texture.Height;

        public Matrix TextureTransform { get; set; } = Matrix.Identity;

        internal override void CalculateLocalToWorld(out Matrix transform)
        {
            var pos = Position - Origin;

            // Calculate local to world transform.
            transform = Matrix.Identity;
            // Scaling.
            transform.M11 = Range;
            transform.M22 = Range;
            // Translation.
            transform.M41 = pos.X;
            transform.M42 = pos.Y;
        }

        internal override void CalculateBounds(out BoundingRectangle bounds)
        {
            var halfSize = new Vector2(Width*0.5f, Height*0.5f);

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
