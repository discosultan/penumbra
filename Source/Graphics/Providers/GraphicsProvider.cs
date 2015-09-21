using System;
using Microsoft.Xna.Framework;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Providers
{
    internal abstract class GraphicsProvider : IDisposable
    {        
        protected PenumbraEngine Engine { get; private set; }
        protected int BackBufferWidth { get; private set; }
        protected int BackBufferHeight { get; private set; }

        public virtual void Load(PenumbraEngine engine)
        {
            Engine = engine;

            // Not working due to https://github.com/mono/MonoGame/issues/3572
            Engine.DeviceManager.PreparingDeviceSettings += SizeChanged;

            BackBufferWidth = engine.Device.Viewport.Width;
            BackBufferHeight = engine.Device.Viewport.Height;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Engine.DeviceManager.PreparingDeviceSettings -= SizeChanged;
        }

        protected virtual void OnSizeChanged() { }

        private void SizeChanged(object sender, PreparingDeviceSettingsEventArgs args)
        {
            ChangeSize(
                args.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth,
                args.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight);
        }

        private void ChangeSize(int width, int height)
        {
            BackBufferWidth = width;
            BackBufferHeight = height;
            Logger.Write($"Back buffer size changed to {BackBufferWidth}x{BackBufferHeight}");
            OnSizeChanged();
        }               
    }
}
