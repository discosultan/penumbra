using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    /// <summary>
    /// A <see cref="Light"/> which equally lights the surroundings in all directions.
    /// </summary>
    public class PointLight : Light
    {
        internal sealed override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);
            return renderer._fxPointLightTech;
        }
    }
}
