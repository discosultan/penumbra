using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Penumbra.Graphics.Providers
{
    internal class Camera : RenderProvider
    {
        public Matrix WorldViewProjection = Matrix.Identity;
        public event EventHandler YInverted;

        private readonly Projections _projections;
        private Matrix _ndcToScreen;
        private Matrix _customWorld = Matrix.Identity;

        private bool _loaded;

        public Camera(Projections projections)
        {
            _projections = projections;
        }

        public override void Load(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager)
        {
            base.Load(graphicsDevice, graphicsDeviceManager);
            CalculateNdcToScreen();
            CalculateWorldViewProjection();
            _loaded = true;         
        }        

        public bool InvertedY { get; private set; }

        public Matrix CustomWorld
        {
            get { return _customWorld; }
            set
            {
                _customWorld = value;
                if ((_projections & Projections.Custom) != 0 && _loaded)
                {
                    CalculateWorldViewProjection();
                }
            }
        }

        /// <summary>
        /// Calculates a screen space rectangle based on world space bounds.
        /// </summary>        
        public Rectangle GetScissorRectangle(Light light)
        {
            Rectangle bounds = light.GetBoundingRectangle();                        

            // 1. Transform from world space to NDC min {-1, -1, 0} max {1, 1, 1}           
            var min = new Vector2(bounds.X, bounds.Y);
            var max = new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height);            
            Vector2.Transform(ref min, ref WorldViewProjection, out min);
            Vector2.Transform(ref max, ref WorldViewProjection, out max);

            // 2. Transform from NDC to screen space min {0, 0, 0} max {viewportWidth, viewportHeight, 1}                    
            Vector2.Transform(ref min, ref _ndcToScreen, out min);
            Vector2.Transform(ref max, ref _ndcToScreen, out max);

            Vector2 minResult, maxResult;
            Vector2.Min(ref min, ref max, out minResult);            
            Vector2.Max(ref min, ref max, out maxResult);

            return new Rectangle(
                (int)minResult.X, 
                (int)minResult.Y, 
                (int)(maxResult.X - minResult.X), 
                (int)(maxResult.Y - minResult.Y));
        }        

        protected override void OnSizeChanged()
        {
            CalculateNdcToScreen();
            CalculateWorldViewProjection();
        }

        private void CalculateNdcToScreen()
        {
            _ndcToScreen = Matrix.Invert(Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1));
        }

        private void CalculateWorldViewProjection()
        {
            Matrix wvp = Matrix.Identity;
            if ((_projections & Projections.Custom) != 0)
            {
                wvp *= CustomWorld;
            }
            if ((_projections & Projections.SpriteBatch) != 0)
            {                
                wvp *= Matrix.CreateOrthographicOffCenter(
                    0,
                    GraphicsDevice.PresentationParameters.BackBufferWidth,
                    GraphicsDevice.PresentationParameters.BackBufferHeight,
                    0,
                    0f, 1f);                
            }
            if ((_projections & Projections.OriginCenter_XRight_YUp) != 0)
            {
                wvp *= Matrix.CreateOrthographicOffCenter(
                    -GraphicsDevice.PresentationParameters.BackBufferWidth / 2f,
                    GraphicsDevice.PresentationParameters.BackBufferWidth / 2f,
                    -GraphicsDevice.PresentationParameters.BackBufferHeight / 2f,
                    GraphicsDevice.PresentationParameters.BackBufferHeight / 2f,
                    0f, 1f);
            }
            if ((_projections & Projections.OriginBottomLeft_XRight_YUp) != 0)
            {
                wvp *= Matrix.CreateOrthographicOffCenter(
                    0,
                    GraphicsDevice.PresentationParameters.BackBufferWidth,
                    0,
                    GraphicsDevice.PresentationParameters.BackBufferHeight,
                    0f, 1f);
            }            

            WorldViewProjection = wvp;

            bool previousInvertedY = InvertedY;
            InvertedY = WorldViewProjection.M22 < 0;
            if (InvertedY != previousInvertedY)
            {
                YInverted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
