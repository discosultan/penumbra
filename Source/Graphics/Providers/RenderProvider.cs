using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Providers
{
    internal abstract class RenderProvider : IDisposable
    {
        private GraphicsDeviceManager _graphicsDeviceManager; 

        protected GraphicsDevice GraphicsDevice { get; private set; }                   

        public virtual void Load(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager)
        {
            GraphicsDevice = graphicsDevice;
            _graphicsDeviceManager = graphicsDeviceManager;

            _graphicsDeviceManager.PreparingDeviceSettings += OnPreparingDeviceSettings;
            BackBufferWidth = GraphicsDevice.Viewport.Width;
            BackBufferHeight = GraphicsDevice.Viewport.Height;            
        }

        protected int BackBufferWidth { get; private set; }
        protected int BackBufferHeight { get; private set; }

        private void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            BackBufferWidth = e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth;
            BackBufferHeight = e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight;
            Logger.Write($"Back buffer size changed to {BackBufferWidth}x{BackBufferHeight}");
            OnSizeChanged();
        }

        protected virtual void OnSizeChanged() { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _graphicsDeviceManager.PreparingDeviceSettings -= OnPreparingDeviceSettings;
            }
        }
    }
}
