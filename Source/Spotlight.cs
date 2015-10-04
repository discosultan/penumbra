using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    public sealed class Spotlight : Light
    {
        public Spotlight()
        {
            Origin = new Vector2(0.0f, 0.5f);
        }

        public float Range
        {
            get { return Scale.Y; }
            set { Scale = new Vector2(value * 2.0f); }
        }
        
        public float ConeAngle { get; set; } = MathHelper.PiOver2;
        public float ConeDecay { get; set; } = 0.5f;

        internal override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);

            renderer._fxLightParamConeAngle.SetValue(ConeAngle);
            renderer._fxLightParamConeDecay.SetValue(ConeDecay);

            return renderer._fxSpotLightTech;
        }
    }
}
