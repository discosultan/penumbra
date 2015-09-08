using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Core;

namespace Penumbra.Graphics.Renderers
{
    internal sealed class LightMapRenderer : IDisposable
    {                
        private PenumbraEngine _engine;

        private Effect _fxTexture;
        private BlendState _bsLightMap;
        private StaticVao _fullscreenQuadVao;

        private Color _ambientColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public Color AmbientColor
        {
            get { return new Color(_ambientColor.R, _ambientColor.G, _ambientColor.B); }
            set { _ambientColor = new Color(value, 1f); }
        }

        public void Load(PenumbraEngine engine)
        {                        
            _engine = engine;

            _fxTexture = engine.Content.Load<Effect>("Texture");

            BuildGraphicsResources();
        }

        public void PreRenderSetup()
        {
            // Switch render target to custom scene texture.
            _engine.Device.SetRenderTarget(_engine.Textures.Scene);
        }

        public void RenderSetup()
        {
            // Switch render target to lightmap.
            _engine.Device.SetRenderTarget(_engine.Textures.LightMap);
            // Clear lightmap color, depth and stencil data.
            _engine.Device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target, _ambientColor, 1f, 0);
        }

        public void Render()
        {
            // Switch render target back to default.
            _engine.Device.SetRenderTarget(null);

            _engine.Device.SamplerStates[0] = SamplerState.LinearClamp;
            _engine.Device.DepthStencilState = DepthStencilState.None;
            _engine.Device.RasterizerState = RasterizerState.CullCounterClockwise;

            // Present original scene to backbuffer.            
            _engine.Device.BlendState = BlendState.Opaque;            
            _fxTexture.Parameters["Texture"].SetValue(_engine.Textures.Scene);
            _engine.Device.Draw(_fxTexture, _fullscreenQuadVao);

            // Present lightmap to backbuffer.
            _engine.Device.BlendState = _bsLightMap;
            _fxTexture.Parameters["Texture"].SetValue(_engine.Textures.LightMap);            
            _engine.Device.Draw(_fxTexture, _fullscreenQuadVao);
        }
         
        public void Dispose()
        {
            _fxTexture.Dispose();
            _bsLightMap.Dispose();
            _fullscreenQuadVao.Dispose();            
        }      

        private void BuildGraphicsResources()
        {
            VertexPosition2Texture[] fullscreenQuadVertices =
            {
                new VertexPosition2Texture(new Vector2(-1f, 1f),  new Vector2(0.0f, 0.0f)),
                new VertexPosition2Texture(new Vector2(3f, 1f), new Vector2(2f, 0.0f)),
                new VertexPosition2Texture(new Vector2(-1f, -3f), new Vector2(0.0f, 2f))
            };            
            _fullscreenQuadVao = StaticVao.New(_engine.Device, fullscreenQuadVertices, VertexPosition2Texture.Layout);            

            _bsLightMap = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.SourceColor,
                ColorSourceBlend = Blend.Zero,
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.SourceAlpha,
                AlphaSourceBlend = Blend.Zero,
                ColorWriteChannels = ColorWriteChannels.All
            };
        }
    }
}
