using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Core;
using Penumbra.Geometry;

namespace Penumbra.Graphics.Providers
{
    internal class CameraProvider : GraphicsProvider
    {
        public Matrix ViewProjection = Matrix.Identity;
        public BoundingRectangle Bounds = new BoundingRectangle(new Vector2(float.MinValue), new Vector2(float.MaxValue));

        private Projections _projections = Projections.OriginCenter_XRight_YUp | Projections.Custom;
        private Matrix _inverseViewProjection = Matrix.Identity;        
        private Matrix _ndcToScreen = Matrix.Identity;
        private Matrix _custom = Matrix.Identity;
        private bool _loaded;                

        public bool InvertedY { get; private set; }

        public Projections Projections
        {
            get { return _projections; }
            set
            {
                _projections = value;
                if (_loaded)
                    CalculateViewProjectionAndBounds();
            }
        }

        public Matrix Custom
        {
            get { return _custom; }
            set
            {
                _custom = value;
                if ((_projections & Projections.Custom) != 0 && _loaded)
                    CalculateViewProjectionAndBounds();
            }
        }

        public override void Load(PenumbraEngine engine)
        {
            base.Load(engine);

            CalculateNdcToScreen();
            CalculateViewProjectionAndBounds();

            _loaded = true;
        }

        /// <summary>
        /// Calculates a screen space rectangle based on world space bounds.
        /// </summary>        
        public BoundingRectangle GetScissorRectangle(Light light)
        {            
            BoundingRectangle bounds = light.Bounds;                

            // 1. Transform from world space to NDC min {-1, -1, 0} max {1, 1, 1}                       
            Vector2.Transform(ref bounds.Min, ref ViewProjection, out bounds.Min);
            Vector2.Transform(ref bounds.Max, ref ViewProjection, out bounds.Max);

            // 2. Transform from NDC to screen space min {0, 0, 0} max {viewportWidth, viewportHeight, 1}                    
            Vector2.Transform(ref bounds.Min, ref _ndcToScreen, out bounds.Min);
            Vector2.Transform(ref bounds.Max, ref _ndcToScreen, out bounds.Max);

            Vector2 min, max;
            Vector2.Min(ref bounds.Min, ref bounds.Max, out min);            
            Vector2.Max(ref bounds.Min, ref bounds.Max, out max);

            // There seem to be 1 pixel offset errors converting rectangle from world -> ndc -> screen.
            // Current fix is to add a a small margin to the screen rectangle.
            const float margin = 1f;
            min += new Vector2(margin);
            max -= new Vector2(margin);

            return new BoundingRectangle(min, max);
        }        

        protected override void OnSizeChanged()
        {
            CalculateNdcToScreen();
            CalculateViewProjectionAndBounds();
        }

        private void CalculateNdcToScreen()
        {
            PresentationParameters pp = Engine.Device.PresentationParameters;
            _ndcToScreen = Matrix.Invert(Matrix.CreateOrthographicOffCenter(0, pp.BackBufferWidth, pp.BackBufferHeight, 0, 0, 1));
        }

        private void CalculateViewProjectionAndBounds()
        {
            // Calculate viewprojection.
            PresentationParameters pp = Engine.Device.PresentationParameters;

            ViewProjection = Matrix.Identity;
            if ((_projections & Projections.Custom) != 0)
                ViewProjection *= Custom;
            if ((_projections & Projections.SpriteBatch) != 0)
                ViewProjection *= Matrix.CreateOrthographicOffCenter(
                    0,
                    pp.BackBufferWidth,
                    pp.BackBufferHeight,
                    0,
                    0f, 1f);
            if ((_projections & Projections.OriginCenter_XRight_YUp) != 0)
                ViewProjection *= Matrix.CreateOrthographicOffCenter(
                    -pp.BackBufferWidth / 2f,
                    pp.BackBufferWidth / 2f,
                    -pp.BackBufferHeight / 2f,
                    pp.BackBufferHeight / 2f,
                    0f, 1f);
            if ((_projections & Projections.OriginBottomLeft_XRight_YUp) != 0)
                ViewProjection *= Matrix.CreateOrthographicOffCenter(
                    0,
                    pp.BackBufferWidth,
                    0,
                    pp.BackBufferHeight,
                    0f, 1f);
            
            // Calculate inversion of viewprojection.
            Matrix.Invert(ref ViewProjection, out _inverseViewProjection);

            // Determine if we are dealing with an inverted (upside down) Y axis.
            InvertedY = ViewProjection.M22 < 0;

            // Calculate fustum bounds.
            Vector2 c1 = Vector2.Transform(new Vector2(+1.0f, +1.0f), _inverseViewProjection);
            Vector2 c2 = Vector2.Transform(new Vector2(-1.0f, -1.0f), _inverseViewProjection);

            Vector2 min, max;
            Vector2.Min(ref c1, ref c2, out min);
            Vector2.Max(ref c1, ref c2, out max);

            Bounds = new BoundingRectangle(min, max);
        }
    }
}
