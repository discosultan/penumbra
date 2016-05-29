using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    /// <summary>
    /// A <see cref="Light"/> emitting light only in a single direction (similar to flashlight).
    /// </summary>
    /// <remarks>
    /// Default direction is to the right. Use <see cref="Light.Rotation"/> to control
    /// in which direction the spotlight is aimed at.
    /// </remarks>
    public class Spotlight : Light
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
        /// A higher value means softer edges. Default is 1.5.
        /// </summary>
        public float ConeDecay { get; set; } = 1.5f;

        internal sealed override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);

            // MathHelper.Pi - 2 x ArcTan reduced.
            float halfAngle = MathHelper.PiOver2 - (float) Math.Atan(2 * Scale.X / Scale.Y);

            renderer._fxLightParamConeAngle.SetValue(halfAngle);
            renderer._fxLightParamConeDecay.SetValue(ConeDecay);

            return renderer._fxSpotLightTech;
        }
    }
}
