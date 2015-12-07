using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Penumbra.Graphics.Effects
{
    internal class LightEffect : IDisposable
    {        
        private EffectParameter _fxLightParamWvp;
        private EffectParameter _fxLightParamTexture;
        private EffectParameter _fxLightParamLightColor;
        private EffectParameter _fxLightParamIntensity;
        private EffectParameter _fxLightParamConeDecay;
        private EffectParameter _fxLightParamConeAngle;
        private EffectParameter _fxLightParamColor;

        public EffectPass PointLightPass { get; private set; }
        public EffectPass SpotlightPass { get; private set; }
        public EffectPass TexturedLightPass { get; private set; }
        public EffectPass DebugLightPass { get; private set; }

        public void SetWorldViewProjection(ref Matrix value) => _fxLightParamWvp.SetValue(value);
        public void SetLightTexture(Texture2D value) => _fxLightParamTexture.SetValue(value);
        public void SetLightColor(ref Vector3 value) => _fxLightParamLightColor.SetValue(value);        
        public void SetLightIntensity(float value) => _fxLightParamIntensity.SetValue(value);        
        public void SetConeHalfAngle(float value) => _fxLightParamConeAngle.SetValue(value);
        public void SetConeDecay(float value) => _fxLightParamConeDecay.SetValue(value);        
        public void SetDebugColor(ref Vector4 value) => _fxLightParamColor.SetValue(value);        

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            Effect = EffectManager.LoadEffectFromEmbeddedResource(graphicsDevice, EffectName);
            BindEffectMembers();
        }        

        public void Dispose()
        {
            Effect?.Dispose();
        }

        protected Effect Effect { get; private set; }
        protected virtual string EffectName { get; } = "Light";

        protected virtual void BindEffectMembers()
        {
            PointLightPass = Effect.Techniques["PointLight"].Passes[0];
            SpotlightPass = Effect.Techniques["Spotlight"]?.Passes[0];
            TexturedLightPass = Effect.Techniques["TexturedLight"]?.Passes[0];
            DebugLightPass = Effect.Techniques["DebugLight"]?.Passes[0];
            _fxLightParamWvp = Effect.Parameters["WorldViewProjection"];
            _fxLightParamTexture = Effect.Parameters["LightTexture"];
            _fxLightParamLightColor = Effect.Parameters["LightColor"];
            _fxLightParamIntensity = Effect.Parameters["LightIntensity"];
            _fxLightParamConeAngle = Effect.Parameters["ConeHalfAngle"];
            _fxLightParamConeDecay = Effect.Parameters["ConeDecay"];
            _fxLightParamColor = Effect.Parameters["DebugColor"];
        }
    }
}
