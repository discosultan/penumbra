using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    /// <inheritdoc />
    public class PointLight : Light
    {
        internal sealed override EffectTechnique ApplyEffectParams(LightRenderer renderer, bool isNormalMapped)
        {
            base.ApplyEffectParams(renderer, isNormalMapped);
            
            return isNormalMapped ? renderer._fxPointLightNormalTech : renderer._fxPointLightTech;
        }
    }
}
