using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Providers
{
    internal class LightmapTextureBuffer : RenderProvider
    {                
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

            LightMap = new RenderTarget2D(GraphicsDevice, BackBufferWidth, BackBufferHeight, false,
                SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8);                

            Logger.Write("New lightmap textures created");
        }

        private void DestroyLightmaps()
        {
            Util.Dispose(LightMap);            
        }
    }
}
