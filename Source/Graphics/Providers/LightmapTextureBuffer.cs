using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Providers
{
    internal class LightmapTextureBuffer : RenderProvider
    {
        public RenderTarget2D Scene { get; private set; }
        public RenderTarget2D LightMap { get; private set; }        

        public override void Load(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager)
        {
            base.Load(graphicsDevice, graphicsDeviceManager);
            CreateLightmapTextures();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DestroyLightmaps();
            }
            base.Dispose(disposing);
        }

        protected override void OnSizeChanged()
        {
            CreateLightmapTextures();
        }

        private void CreateLightmapTextures()
        {
            DestroyLightmaps();

            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            //LightMap = new RenderTarget2D(GraphicsDevice, BackBufferWidth, BackBufferHeight);
            LightMap = new RenderTarget2D(GraphicsDevice, BackBufferWidth, BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            Scene = new RenderTarget2D(GraphicsDevice, BackBufferWidth, BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            Logger.Write("New lightmap textures created");
        }

        private void DestroyLightmaps()
        {
            Util.Dispose(LightMap);            
        }
    }
}
