using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    public sealed class PointLight : Light
    {
        internal override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);

            return renderer._fxPointLightTech;
        }

        internal override void CalculateLocalToWorld(out Matrix transform)
        {
            // Calculate local to world transform.
            transform = Matrix.Identity;
            // Scaling.
            transform.M11 = Range;
            transform.M22 = Range;
            // Translation.
            transform.M41 = Position.X;
            transform.M42 = Position.Y;
        }

        internal override void CalculateBounds(out BoundingRectangle bounds)
        {
            var rangeVector = new Vector2(Range);
            Vector2 min = Position - rangeVector;
            Vector2 max = Position + rangeVector;
            bounds = new BoundingRectangle(min, max);
        }
    }
}
