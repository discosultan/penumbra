using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Providers
{
    internal class TextureProvider : GraphicsProvider
    {
        public RenderTarget2D Scene { get; private set; }
        public RenderTarget2D LightMap { get; private set; }        

        public override void Load(PenumbraEngine engine)
        {
            base.Load(engine);
            BuildRenderTargets();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DestroyRenderTargets();
            base.Dispose(disposing);
        }

        protected override void OnSizeChanged()
        {
            BuildRenderTargets();
        }

        private void BuildRenderTargets()
        {
            DestroyRenderTargets();

            PresentationParameters pp = Engine.Device.PresentationParameters;
            
            LightMap = new RenderTarget2D(Engine.Device, BackBufferWidth, BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            Scene = new RenderTarget2D(Engine.Device, BackBufferWidth, BackBufferHeight, false,
                pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            Logger.Write("New lightmap textures created");
        }

        private void DestroyRenderTargets()
        {
            Scene?.Dispose();
            LightMap?.Dispose();
        }
    }
}
