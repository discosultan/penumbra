using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics.Providers
{
    internal class Camera : RenderProvider
    {                
        private Matrix _vp;
        private Matrix _ndcToScreen;

        public override void Load(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager)
        {
            base.Load(graphicsDevice, graphicsDeviceManager);
            CalculateNdcToScreen();
        }

        public Matrix ViewProjection
        {
            get { return _vp; }
            set { _vp = value; }
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
            Vector2.Transform(ref min, ref _vp, out min);
            Vector2.Transform(ref max, ref _vp, out max);

            // 2. Transform from NDC to screen space min {0, 0, 0} max {viewportWidth, viewportHeight, 1}                    
            Vector2.Transform(ref min, ref _ndcToScreen, out min);
            Vector2.Transform(ref max, ref _ndcToScreen, out max);

            Vector2 minResult = Vector2.Min(min, max);
            Vector2 maxResult = Vector2.Max(min, max);

            return new Rectangle(
                (int)minResult.X, 
                (int)minResult.Y, 
                (int)(maxResult.X - minResult.X), 
                (int)(maxResult.Y - minResult.Y));
        }

        protected override void OnSizeChanged()
        {
            CalculateNdcToScreen();
        }

        private void CalculateNdcToScreen()
        {
            _ndcToScreen = Matrix.Invert(Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1));
        }
    }
}
