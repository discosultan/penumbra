using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    public sealed class PointLight : Light
    {
        //private float _range = 100.0f;
        ///// <summary>
        ///// Gets or sets how far from the position the light reaches (falls off).
        ///// </summary>
        //public float Range
        //{
        //    get { return _range; }
        //    set
        //    {
        //        if (_range != value)
        //        {
        //            _range = value;
        //            _worldDirty = true;
        //        }
        //    }
        //}

        internal override void CalculateLocalToWorld(out Matrix transform)
        {            
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

        internal override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);

            return renderer._fxPointLightTech;
        }
    }
}
