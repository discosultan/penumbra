using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    /// <inheritdoc />
    public sealed class Spotlight : Light
    {
        /// <summary>
        /// Constructs a new instance of <see cref="Spotlight"/>.
        /// </summary>
        public Spotlight()
        {
            Origin = new Vector2(0.0f, 0.5f);
        }

        /// <summary>
        /// Gets or sets the rate of cone attenuation to the sides.
        /// </summary>
        public float ConeDecay { get; set; } = 0.5f;

        internal override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);
            
            float halfAngle = MathHelper.PiOver2 - (float) Math.Atan(2 * Scale.X / Scale.Y); // MathHelper.Pi - 2 x ArcTan reduced.

            renderer._fxLightParamConeAngle.SetValue(halfAngle);
            renderer._fxLightParamConeDecay.SetValue(ConeDecay);

            return renderer._fxSpotLightTech;
        }
    }
}
