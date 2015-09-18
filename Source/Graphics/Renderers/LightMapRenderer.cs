using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics.Renderers
{
    internal sealed class LightMapRenderer : IDisposable
    {                
        private PenumbraEngine _engine;

        private Effect _fxTexture;
        private EffectParameter _fxTextureParamTexture;
        private BlendState _bsLightMap;
        private StaticVao _fullscreenQuadVao;

        public void Load(PenumbraEngine engine)
        {                        
            _engine = engine;

            _fxTexture = engine.Content.Load<Effect>("Texture");
            _fxTextureParamTexture = _fxTexture.Parameters["Texture"];

            BuildGraphicsResources();
        }

        public void Present()
        {            
            _engine.Device.DepthStencilState = DepthStencilState.None;
            _engine.Device.RasterizerState = RasterizerState.CullCounterClockwise;

            // Present original scene to backbuffer.            
            _engine.Device.BlendState = BlendState.Opaque;
            _fxTextureParamTexture.SetValue(_engine.Textures.Scene);
            _engine.Device.SetVertexArrayObject(_fullscreenQuadVao);
            _fxTexture.CurrentTechnique.Passes[0].Apply();
            _engine.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _fullscreenQuadVao.VertexCount - 2);

            // Present lightmap to backbuffer.
            _engine.Device.BlendState = _bsLightMap;
            _fxTextureParamTexture.SetValue(_engine.Textures.LightMap);
            _engine.Device.SetVertexArrayObject(_fullscreenQuadVao);
            _fxTexture.CurrentTechnique.Passes[0].Apply();
            _engine.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _fullscreenQuadVao.VertexCount - 2);
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
