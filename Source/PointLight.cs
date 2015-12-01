using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    /// <inheritdoc />
    public class PointLight : Light
    {        
        internal sealed override EffectPass ApplyEffectParams(LightRenderer renderer, bool isNormalMapped)
        {
            base.ApplyEffectParams(renderer, isNormalMapped);
            return isNormalMapped 
                ? renderer._fxLightNormalPassPoint 
                : renderer._fxLightPassPoint;
        }
    }
}
