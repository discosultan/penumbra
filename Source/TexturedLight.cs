using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    public sealed class TexturedLight : Light
    {
        /// <summary>
        /// Gets or sets the texture used to determine in what shape to render the light.
        /// A spotlight could be simulated with a spotlight texture. If no texture is set,
        /// uses a linear falloff equation to render a point light shaped light. 
        /// </summary>
        public Texture Texture { get; set; }

        public Matrix TextureTransform { get; set; } = Matrix.Identity;

        internal override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);

            renderer._fxLightParamTexture.SetValue(Texture);
            renderer._fxLightParamTextureTransform.SetValue(TextureTransform);

            return renderer._fxTexturedLightTech;
        }
    }
}
