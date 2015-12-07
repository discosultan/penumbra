using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Effects;

namespace Penumbra.Graphics.Renderers
{
    internal class LightRenderer : IDisposable
    {        
        private static Vector4 DebugColor = Color.Green.ToVector4();

        private PenumbraEngine _engine;

        public LightEffect LightEffect { get; } = new LightEffect();
        public NormalMappedLightEffect NormalMappedLightEffect { get; } = new NormalMappedLightEffect();

        private StaticVao _quadVao;
        private StaticVao _circleVao;
        private BlendState _bsLight;
        private DepthStencilState _dssOccludedLight;

        public void Initialize(PenumbraEngine engine)
        {
            _engine = engine;

            LightEffect.Initialize(_engine.GraphicsDevice);
            NormalMappedLightEffect.Initialize(_engine.GraphicsDevice);

            // Constant shader param.
            LightEffect.SetDebugColor(ref DebugColor);

            BuildGraphicsResources();
        }        

        public void PreRender()
        {
            if (_engine.NormalMappedLightingEnabled)
            {
                //_fxLightNormalParamWvp.SetValue(_engine.Camera.ViewProjection);                                
                NormalMappedLightEffect.SetNormalMap(_engine.Textures.NormalMap);
                //_fxLightNormal.Parameters["Resolution"].SetValue(new Vector2(_engine.GraphicsDevice.Viewport.Width, _engine.GraphicsDevice.Viewport.Height));
            }
            else
            {
                //_fxLightParamViewProjection.SetValue(_engine.Camera.ViewProjection);
            }
        }

        public void Render(Light light)
        {
            EffectPass fxPass = light.ApplyEffectParams(this, _engine.NormalMappedLightingEnabled);        

            Matrix wvp;
            Matrix.Multiply(ref light.LocalToWorld, ref _engine.Camera.ViewProjection, out wvp);

            _engine.GraphicsDevice.DepthStencilState = light.ShadowType == ShadowType.Occluded
                ? _dssOccludedLight
                : DepthStencilState.None;
            _engine.GraphicsDevice.BlendState = _bsLight;
            _engine.GraphicsDevice.RasterizerState = _engine.RasterizerState;
            _engine.GraphicsDevice.SetVertexArrayObject(_quadVao);
            
            // TODO: fix
            LightEffect.SetWorldViewProjection(ref wvp);
            NormalMappedLightEffect.SetWorldViewProjection(ref wvp);

            fxPass.Apply();
            _engine.GraphicsDevice.DrawPrimitives(_quadVao.PrimitiveTopology, 0, _quadVao.PrimitiveCount);

            if (_engine.Debug)
            {
                _engine.GraphicsDevice.BlendState = BlendState.Opaque;
                _engine.GraphicsDevice.RasterizerState = _engine.RasterizerStateDebug;

                // Draw debug quad.
                //const float factor = 0.41f;
                //wvp.M11 = wvp.M11 * factor;
                //wvp.M22 = wvp.M22 * factor;

                //_fxLightParamWvp.SetValue(wvp);
                LightEffect.DebugLightPass.Apply();
                _engine.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _quadVao.VertexCount - 2);



                //Matrix world = Matrix.Identity;
                //// Scaling.
                //world.M11 = light.Radius;
                //world.M22 = light.Radius;
                //// Translation.
                //world.M41 = light.Position.X;
                //world.M42 = light.Position.Y;
                //Matrix.Multiply(ref world, ref _engine.Camera.ViewProjection, out wvp);

                //_engine.GraphicsDevice.SetVertexArrayObject(_circleVao);
                //_fxLightParamWvp.SetValue(wvp);
                //_fxLightPassDebug.Apply();
                //_engine.GraphicsDevice.DrawIndexedPrimitives(_circleVao.PrimitiveTopology, 0, 0, _circleVao.VertexCount, 0, _circleVao.PrimitiveCount);
            }
        }

        public void Dispose()
        {
            LightEffect.Dispose();
            NormalMappedLightEffect.Dispose();
            _quadVao?.Dispose();
            _circleVao?.Dispose();
            _bsLight?.Dispose();
        }

        private void BuildGraphicsResources()
        {            
            // We build the quad a little larger than required in order to be able to also properly clear the alpha
            // for the region. The reason we need larger quad is due to camera rotation.
            var d = (float) (1 / Math.Sqrt(2));
            //d = 0;

            // Quad.
            VertexPosition2Texture[] quadVertices =
            {
                new VertexPosition2Texture(new Vector2(0.0f - d, 1.0f + d), new Vector2(0.0f - d, 0.0f - d)),
                new VertexPosition2Texture(new Vector2(1.0f + d, 1.0f + d), new Vector2(1.0f + d, 0.0f - d)),
                new VertexPosition2Texture(new Vector2(0.0f - d, 0.0f - d), new Vector2(0.0f - d, 1.0f + d)),
                new VertexPosition2Texture(new Vector2(1.0f + d, 0.0f - d), new Vector2(1.0f + d, 1.0f + d))
            };            

            _quadVao = StaticVao.New(_engine.GraphicsDevice, quadVertices, VertexPosition2Texture.Layout, PrimitiveType.TriangleStrip);

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

            _circleVao = StaticVao.New(_engine.GraphicsDevice, vertices, VertexPosition2Texture.Layout, PrimitiveType.TriangleList, indices);

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
