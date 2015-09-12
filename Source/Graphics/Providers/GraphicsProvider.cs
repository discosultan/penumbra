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

            engine.DeviceManager.PreparingDeviceSettings += OnPreparingDeviceSettings;
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
                Engine.DeviceManager.PreparingDeviceSettings -= OnPreparingDeviceSettings;
        }

        protected virtual void OnSizeChanged() { }

        private void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            BackBufferWidth = e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth;
            BackBufferHeight = e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight;
            Logger.Write($"Back buffer size changed to {BackBufferWidth}x{BackBufferHeight}");
            OnSizeChanged();
        }               
    }
}
