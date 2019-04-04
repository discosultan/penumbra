using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics.Renderers
{
    internal class LightRenderer : IDisposable
    {
        private static readonly Vector4 DebugColor = Color.Green.ToVector4();

        private PenumbraEngine _engine;

        private Effect _fxLight;
        internal EffectTechnique _fxPointLightTech;
        internal EffectTechnique _fxSpotLightTech;
        internal EffectTechnique _fxTexturedLightTech;
        internal EffectTechnique _fxDebugLightTech;
        internal EffectParameter _fxLightParamTexture;
        internal EffectParameter _fxLightParamTextureTransform;
        internal EffectParameter _fxLightParamWvp;
        internal EffectParameter _fxLightParamColor;
        internal EffectParameter _fxLightParamIntensity;
        internal EffectParameter _fxLightParamConeDecay;
        internal EffectParameter _fxLightParamConeAngle;
        private StaticVao _quadVao;
        private StaticVao _circleVao;
        private BlendState _bsLight;
        private DepthStencilState _dssOccludedLight;

        public void Load(PenumbraEngine engine, Effect fxLight)
        {
            _engine = engine;

            _fxLight = fxLight;
            _fxPointLightTech = _fxLight.Techniques["PointLight"];
            _fxSpotLightTech = _fxLight.Techniques["Spotlight"];
            _fxTexturedLightTech = _fxLight.Techniques["TexturedLight"];
            _fxDebugLightTech = _fxLight.Techniques["DebugLight"];
#if DESKTOPGL
            _fxLightParamTexture = _fxLight.Parameters["TextureSampler+Texture"];
#else
            _fxLightParamTexture = _fxLight.Parameters["Texture"];
#endif
            _fxLightParamTextureTransform = _fxLight.Parameters["TextureTransform"];
            _fxLightParamWvp = _fxLight.Parameters["WorldViewProjection"];
            _fxLightParamColor = _fxLight.Parameters["LightColor"];
            _fxLightParamIntensity = _fxLight.Parameters["LightIntensity"];
            _fxLightParamConeAngle = _fxLight.Parameters["ConeHalfAngle"];
            _fxLightParamConeDecay = _fxLight.Parameters["ConeDecay"];

            // Constant shader param.
            _fxLight.Parameters["Color"].SetValue(DebugColor);

            BuildGraphicsResources();
        }

        public void Render(Light light)
        {
            EffectTechnique fxTech = light.ApplyEffectParams(this);

            Matrix.Multiply(ref light.LocalToWorld, ref _engine.Camera.ViewProjection, out Matrix wvp);

            _engine.Device.DepthStencilState = DepthStencilState.None;
            _engine.Device.DepthStencilState = light.ShadowType == ShadowType.Occluded
                ? _dssOccludedLight
                : DepthStencilState.None;
            _engine.Device.BlendState = _bsLight;
            _engine.Device.RasterizerState = _engine.Rs;
            _engine.Device.SetVertexArrayObject(_quadVao);
            _fxLightParamWvp.SetValue(wvp);
            fxTech.Passes[0].Apply();
            _engine.Device.DrawPrimitives(_quadVao.PrimitiveTopology, 0, _quadVao.PrimitiveCount);

            if (_engine.Debug)
            {
                _engine.Device.BlendState = BlendState.Opaque;
                _engine.Device.RasterizerState = _engine.RsDebug;

                // Draw debug quad.
                //const float factor = 0.41f;
                //wvp.M11 = wvp.M11 * factor;
                //wvp.M22 = wvp.M22 * factor;

                //_fxLightParamWvp.SetValue(wvp);
                //_fxDebugLightTech.Passes[0].Apply();
                //_engine.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _quadVao.VertexCount - 2);

                Matrix world = Matrix.Identity;
                // Scaling.
                world.M11 = light.Radius;
                world.M22 = light.Radius;
                // Translation.
                world.M41 = light.Position.X;
                world.M42 = light.Position.Y;
                Matrix.Multiply(ref world, ref _engine.Camera.ViewProjection, out wvp);

                _engine.Device.SetVertexArrayObject(_circleVao);
                _fxLightParamWvp.SetValue(wvp);
                _fxDebugLightTech.Passes[0].Apply();
                _engine.Device.DrawIndexedPrimitives(_circleVao.PrimitiveTopology, 0, 0, _circleVao.PrimitiveCount);
            }
        }

        public void Dispose()
        {
            _fxLight?.Dispose();
            _quadVao?.Dispose();
            _circleVao?.Dispose();
            _bsLight?.Dispose();
        }

        private void BuildGraphicsResources()
        {
            // We build the quad a little larger than required in order to be able to also properly clear the alpha
            // for the region. The reason we need larger quad is due to camera rotation.
            var d = (float) (1 / Math.Sqrt(2));

            // Quad.
            VertexPosition2Texture[] quadVertices =
            {
                new VertexPosition2Texture(new Vector2(0.0f - d, 1.0f + d), new Vector2(0.0f - d, 0.0f - d)),
                new VertexPosition2Texture(new Vector2(1.0f + d, 1.0f + d), new Vector2(1.0f + d, 0.0f - d)),
                new VertexPosition2Texture(new Vector2(0.0f - d, 0.0f - d), new Vector2(0.0f - d, 1.0f + d)),
                new VertexPosition2Texture(new Vector2(1.0f + d, 0.0f - d), new Vector2(1.0f + d, 1.0f + d))
            };

            _quadVao = StaticVao.New(_engine.Device, quadVertices, VertexPosition2Texture.Layout, PrimitiveType.TriangleStrip);

            // Circle.
            const int circlePoints = 12;
            const float radius = 1.0f;
            const float rotationIncrement = MathHelper.TwoPi / circlePoints;

            var vertices = new VertexPosition2Texture[circlePoints + 1];
            var indices = new int[circlePoints * 3];

            var center = new VertexPosition2Texture(Vector2.Zero, Vector2.One);
            vertices[0] = center;
            for (int i = 1; i <= circlePoints; i++)
            {
                var angle = rotationIncrement * i;
                vertices[i] = new VertexPosition2Texture(new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius), Vector2.Zero);

                int indexStart = (i - 1) * 3;
                indices[indexStart++] = 0;
                indices[indexStart++] = i;
                indices[indexStart] = i + 1;
            }
            indices[indices.Length - 1] = 1;

            _circleVao = StaticVao.New(_engine.Device, vertices, VertexPosition2Texture.Layout, PrimitiveType.TriangleList, indices);

            // Render states.
            _bsLight = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationAlpha,
                ColorDestinationBlend = Blend.One,
                //ColorDestinationBlend = Blend.InverseSourceColor, // Blends lights a little softer.
                ColorWriteChannels = ColorWriteChannels.All,
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.Zero
            };
            _dssOccludedLight = new DepthStencilState
            {
                DepthBufferEnable = false,

                StencilEnable = true,
                StencilWriteMask = 0xff,
                StencilMask = 0x00,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Zero
            };
        }
    }
}
