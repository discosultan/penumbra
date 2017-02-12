using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Providers
{
    internal abstract class GraphicsProvider : IDisposable
    {
        protected PenumbraEngine Engine { get; private set; }
        protected int ViewportWidth { get; private set; }
        protected int ViewportHeight { get; private set; }

        public virtual void Load(PenumbraEngine engine)
        {
            Engine = engine;

            // Not working due to https://github.com/mono/MonoGame/issues/3572
            //Engine.DeviceManager.PreparingDeviceSettings += PreparingDeviceSettings;
            Engine.Window.ClientSizeChanged += ClientSizeChanged;

            ViewportWidth = engine.Device.Viewport.Width;
            ViewportHeight = engine.Device.Viewport.Height;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Engine.DeviceManager.PreparingDeviceSettings -= PreparingDeviceSettings;
                Engine.Window.ClientSizeChanged -= ClientSizeChanged;
            }
        }

        protected virtual void OnSizeChanged() { }

        private void PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args)
        {
            PresentationParameters pp = args.GraphicsDeviceInformation.PresentationParameters;
            ChangeSize(pp.BackBufferWidth, pp.BackBufferHeight);
        }

        private void ClientSizeChanged(object sender, EventArgs e)
        {
            ChangeSize(Engine.Device.Viewport.Width, Engine.Device.Viewport.Height);
        }

        private void ChangeSize(int width, int height)
        {
            if (ViewportWidth != width || ViewportHeight != height)
            {
                ViewportWidth = width;
                ViewportHeight = height;
                Logger.Write($"Viewport size changed to {ViewportWidth}x{ViewportHeight}");
                OnSizeChanged();
            }
        }
    }
}
