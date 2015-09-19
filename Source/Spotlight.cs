using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Renderers;

namespace Penumbra
{
    public sealed class Spotlight : Light
    {
        private Vector2 _coneDirection = Vector2.UnitY;

        public Vector2 ConeDirection
        {
            get { return _coneDirection; }
            set
            {
                if (value == Vector2.Zero)
                    value = Vector2.UnitY;                
                value.Normalize();            
                _coneDirection = value;
            }
        }
        public float ConeAngle { get; set; } = MathHelper.PiOver2;
        public float ConeDecay { get; set; } = 0.5f;

        internal override EffectTechnique ApplyEffectParams(LightRenderer renderer)
        {
            base.ApplyEffectParams(renderer);

            renderer._fxLightParamConeAngle.SetValue(ConeAngle);
            renderer._fxLightParamConeDecay.SetValue(ConeDecay);
            renderer._fxLightParamConeDirection.SetValue(ConeDirection);

            return renderer._fxSpotLightTech;
        }
    }
}
