using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics.Effects
{
    internal sealed class NormalMappedLightEffect : LightEffect
    {
        private EffectParameter _fxLightNormalParamNormalMap;
        private EffectParameter _fxLightNormalParamWorld;
        private EffectParameter _fxLightNormalParamLightPosition;

        public void SetNormalMap(Texture2D value) => _fxLightNormalParamNormalMap.SetValue(value);        
        public void SetWorld(ref Matrix value) => _fxLightNormalParamWorld.SetValue(value);
        public void SetLightPosition(ref Vector3 value) => _fxLightNormalParamLightPosition.SetValue(value);

        protected override string EffectName { get; } = "LightNormalMapped";

        protected override void BindEffectMembers()
        {
            base.BindEffectMembers();

            _fxLightNormalParamNormalMap = Effect.Parameters["NormalMap"];
            _fxLightNormalParamWorld = Effect.Parameters["World"];
            _fxLightNormalParamLightPosition = Effect.Parameters["LightPosition"];
        }
    }
}
