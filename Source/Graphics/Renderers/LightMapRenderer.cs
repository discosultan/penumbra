using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics.Renderers
{
    internal sealed class LightmapRenderer : IDisposable
    {                
        private PenumbraEngine _engine;

        private Effect _fxTexture;
        private EffectParameter _fxTextureParamDiffuseMap;
        private EffectParameter _fxTextureParamLightmap;
        private StaticVao _fullscreenQuadVao;

        public void Initialize(PenumbraEngine engine)
        {                        
            _engine = engine;

            _fxTexture = EffectManager.LoadEffectFromEmbeddedResource(_engine.GraphicsDevice, "Texture");
            _fxTextureParamDiffuseMap = _fxTexture.Parameters["DiffuseMap"];
            _fxTextureParamLightmap = _fxTexture.Parameters["Lightmap"];

            BuildGraphicsResources();
        }

        public void Render()
        {            
            // Blend diffuse map and lightmap together and present to original render target.
            _engine.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            _engine.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;            
            _engine.GraphicsDevice.BlendState = BlendState.Opaque;
            _engine.GraphicsDevice.SetVertexArrayObject(_fullscreenQuadVao);
            _fxTextureParamDiffuseMap.SetValue(_engine.Textures.DiffuseMap);
            _fxTextureParamLightmap.SetValue(_engine.Textures.Lightmap);
            _fxTexture.CurrentTechnique.Passes[0].Apply();
            _engine.GraphicsDevice.DrawPrimitives(_fullscreenQuadVao.PrimitiveTopology, 0, _fullscreenQuadVao.PrimitiveCount);
        }
         
        public void Dispose()
        {
            _fxTexture?.Dispose();            
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
            _fullscreenQuadVao = StaticVao.New(_engine.GraphicsDevice, fullscreenQuadVertices, VertexPosition2Texture.Layout, PrimitiveType.TriangleStrip);
        }
    }
}
