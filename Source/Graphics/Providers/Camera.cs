using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Penumbra.Core;
using Penumbra.Geometry;

namespace Penumbra.Graphics.Providers
{
    internal class Camera : RenderProvider
    {
        public event EventHandler YInverted;

        public Matrix WorldViewProjection = Matrix.Identity;
        private Matrix _inverseWorldViewProjection = Matrix.Identity;

        private readonly Projections _projections;
        private Matrix _ndcToScreen;
        private Matrix _custom = Matrix.Identity;
        private bool _loaded;

        public Camera(Projections projections)
        {
            _projections = projections;
        }

        public override void Load(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager)
        {
            base.Load(graphicsDevice, graphicsDeviceManager);
            OnSizeChanged();        
            _loaded = true;         
        }

        public BoundingRectangle Bounds { get; private set; }

        public bool InvertedY { get; private set; }

        public Matrix Custom
        {
            get { return _custom; }
            set
            {
                _custom = value;
                if ((_projections & Projections.Custom) != 0 && _loaded)
                {
                    CalculateWorldViewProjection();
                    CalculateBounds();
                }
            }
        }        

        /// <summary>
        /// Calculates a screen space rectangle based on world space bounds.
        /// </summary>        
        public BoundingRectangle GetScissorRectangle(Light light)
        {            
            BoundingRectangle bounds = light.Bounds;                

            // 1. Transform from world space to NDC min {-1, -1, 0} max {1, 1, 1}                       
            Vector2.Transform(ref bounds.Min, ref WorldViewProjection, out bounds.Min);
            Vector2.Transform(ref bounds.Max, ref WorldViewProjection, out bounds.Max);

            // 2. Transform from NDC to screen space min {0, 0, 0} max {viewportWidth, viewportHeight, 1}                    
            Vector2.Transform(ref bounds.Min, ref _ndcToScreen, out bounds.Min);
            Vector2.Transform(ref bounds.Max, ref _ndcToScreen, out bounds.Max);

            Vector2 min, max;
            Vector2.Min(ref bounds.Min, ref bounds.Max, out min);            
            Vector2.Max(ref bounds.Min, ref bounds.Max, out max);

            return new BoundingRectangle(min, max);
        }        

        protected override void OnSizeChanged()
        {
            CalculateNdcToScreen();
            CalculateWorldViewProjection();
            CalculateBounds();
        }

        private void CalculateBounds()
        {
            Vector2 c1 = Vector2.Transform(new Vector2(+1.0f, +1.0f), _inverseWorldViewProjection);
            Vector2 c2 = Vector2.Transform(new Vector2(-1.0f, -1.0f), _inverseWorldViewProjection);

            Vector2 min, max;
            Vector2.Min(ref c1, ref c2, out min);
            Vector2.Max(ref c1, ref c2, out max);
            Bounds = new BoundingRectangle(min, max);
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
                wvp *= Custom;
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
            Matrix.Invert(ref WorldViewProjection, out _inverseWorldViewProjection);

            bool previousInvertedY = InvertedY;
            InvertedY = WorldViewProjection.M22 < 0;
            if (InvertedY != previousInvertedY)
            {
                YInverted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
