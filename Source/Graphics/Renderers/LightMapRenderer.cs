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
        private EffectTechnique _fxTechMain;
        private EffectTechnique _fxTechNormal;
        private BlendState _bsLightMap;
        private StaticVao _fullscreenQuadVao;        

        public void Load(PenumbraEngine engine)
        {                        
            _engine = engine;

            _fxTexture = EffectManager.LoadEffectFromEmbeddedResource(_engine.GraphicsDevice, "Texture");
            _fxTechMain = _fxTexture.Techniques["Main"];
            _fxTechNormal = _fxTexture.Techniques["Normal"];
            _fxTextureParamTexture = _fxTexture.Parameters["Texture"];

            BuildGraphicsResources();
        }

        public void Present()
        {            
            _engine.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            _engine.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Present original scene to backbuffer.            
            _engine.GraphicsDevice.BlendState = BlendState.Opaque;
            _engine.GraphicsDevice.SetVertexArrayObject(_fullscreenQuadVao);
            _fxTextureParamTexture.SetValue(_engine.Textures.Scene);            
            _fxTechMain.Passes[0].Apply();
            _engine.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _fullscreenQuadVao.VertexCount - 2);

            // Present lightmap to backbuffer.
            _engine.GraphicsDevice.BlendState = _bsLightMap;
            _fxTextureParamTexture.SetValue(_engine.Textures.LightMap);
            if (_engine.NormalMappedLightingEnabled)
            {
                _fxTexture.Parameters["NormalMap1"].SetValue(_engine.Textures.NormalMap);
                _fxTexture.Parameters["NormalMap2"].SetValue(_engine.Textures.LightMapNormal);
                _fxTechNormal.Passes[0].Apply();
            }
            else
            {
                _fxTechMain.Passes[0].Apply();
            }            
            _engine.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, _fullscreenQuadVao.VertexCount - 2);
        }
         
        public void Dispose()
        {
            _fxTexture?.Dispose();
            _bsLightMap?.Dispose();
            _fullscreenQuadVao?.Dispose();            
        }      

        private void BuildGraphicsResources()
        {
            VertexPosition2Texture[] fullscreenQuadVertices =
            {
                new VertexPosition2Texture(new Vector2(-1.0f, 1.0f),  new Vector2(0.0f, 0.0f)),
                new VertexPosition2Texture(new Vector2(3.0f, 1.0f), new Vector2(2.0f, 0.0f)),
                new VertexPosition2Texture(new Vector2(-1.0f, -3.0f), new Vector2(0.0f, 2.0f))
            };            
            _fullscreenQuadVao = StaticVao.New(_engine.GraphicsDevice, fullscreenQuadVertices, VertexPosition2Texture.Layout);            

            _bsLightMap = new BlendState
            {
                ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue,
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.SourceColor
            };
        }
    }
}
