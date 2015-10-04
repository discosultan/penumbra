using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    public sealed class PointLight : Light
    {
        public float Range
        {
            get { return Scale.X*0.5f; }
            set { Scale = new Vector2(value*2.0f); }
        }

        internal override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);
            return renderer._fxPointLightTech;
        }
    }
}
