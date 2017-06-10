using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    /// <summary>
    /// A <see cref="Light"/> which allows its shape to be determined by a
    /// custom <see cref="Texture2D"/>.
    /// </summary>
    public class TexturedLight : Light
    {
        /// <summary>
        /// Constructs a new instance of <see cref="TexturedLight"/>.
        /// </summary>
        /// <param name="texture">
        /// Texture used to determine light strength at the sampled point.
        /// Pass NULL to set texture later.
        /// </param>
        public TexturedLight(Texture2D texture = null)
        {
            Texture = texture;
            if (Texture != null)
                Scale = new Vector2(Texture.Width, Texture.Height);
        }

        /// <summary>
        /// Gets or sets the texture used to determine in what shape to render the light.
        /// A spotlight could be simulated with a spotlight texture. If no texture is set,
        /// uses a linear falloff equation to render a point light shaped light.
        /// </summary>
        public Texture2D Texture { get; set; }

        internal sealed override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);

            renderer._fxLightParamTexture.SetValue(Texture);

            return renderer._fxTexturedLightTech;
        }
    }
}
