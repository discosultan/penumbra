using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Providers
{
    internal class CameraProvider : GraphicsProvider
    {
        public Matrix ViewProjection = Matrix.Identity;
        public BoundingRectangle Bounds = new BoundingRectangle(new Vector2(float.MinValue), new Vector2(float.MaxValue));

        private Transforms _transforms = Transforms.OriginCenter_XRight_YUp | Transforms.Custom;
        private Matrix _inverseViewProjection = Matrix.Identity;        
        private Matrix _ndcToScreen = Matrix.Identity;
        private Matrix _custom = Matrix.Identity;
        private bool _loaded;

        public bool InvertedY { get; private set; }

        public Transforms Transforms
        {
            get { return _transforms; }
            set
            {
                _transforms = value;
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
                if ((_transforms & Transforms.Custom) != 0 && _loaded)
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
        public void GetScissorRectangle(Light light, out BoundingRectangle scissor)
        {
            Matrix transform;
            Matrix.Multiply(ref ViewProjection, ref _ndcToScreen, out transform);
            
            BoundingRectangle.Transform(ref light.Bounds, ref transform, out scissor);
        }        

        protected override void OnSizeChanged()
        {
            Logger.Write($"Screen size changed to {BackBufferWidth}x{BackBufferHeight}.");
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
            // Calculate view projection transform.
            PresentationParameters pp = Engine.Device.PresentationParameters;

            ViewProjection = Matrix.Identity;
            if ((_transforms & Transforms.Custom) != 0)
                ViewProjection *= Custom;
            if ((_transforms & Transforms.SpriteBatch) != 0)
                ViewProjection *= Matrix.CreateOrthographicOffCenter(
                    0,
                    pp.BackBufferWidth,
                    pp.BackBufferHeight,
                    0,
                    0.0f, 1.0f);
            if ((_transforms & Transforms.OriginCenter_XRight_YUp) != 0)
                ViewProjection *= Matrix.CreateOrthographicOffCenter(
                    -pp.BackBufferWidth / 2.0f,
                    pp.BackBufferWidth / 2.0f,
                    -pp.BackBufferHeight / 2.0f,
                    pp.BackBufferHeight / 2.0f,
                    0.0f, 1.0f);
            if ((_transforms & Transforms.OriginBottomLeft_XRight_YUp) != 0)
                ViewProjection *= Matrix.CreateOrthographicOffCenter(
                    0,
                    pp.BackBufferWidth,
                    0,
                    pp.BackBufferHeight,
                    0.0f, 1.0f);
            //LogViewProjection();

            // Calculate inversion of viewprojection.
            Matrix.Invert(ref ViewProjection, out _inverseViewProjection);

            // Determine if we are dealing with an inverted (upside down) Y axis.
            InvertedY = ViewProjection.M22 < 0 && ViewProjection.M11 >= 0 ||
                        ViewProjection.M22 >= 0 && ViewProjection.M11 < 0;
            
            var size = new Vector2(1.0f);
            var bounds = new BoundingRectangle(-size, size);
            BoundingRectangle.Transform(ref bounds, ref _inverseViewProjection, out Bounds);            
        }

        private void LogViewProjection()
        {
            var m = ViewProjection;
            Logger.Write("-----------------");
            Logger.Write($"{m.M11:+0.0000;-0.0000} {m.M12:+0.0000;-0.0000} {m.M13:+0.0000;-0.0000} {m.M14:+0.0000;-0.0000}");
            Logger.Write($"{m.M21:+0.0000;-0.0000} {m.M22:+0.0000;-0.0000} {m.M23:+0.0000;-0.0000} {m.M24:+0.0000;-0.0000}");
            Logger.Write($"{m.M31:+0.0000;-0.0000} {m.M32:+0.0000;-0.0000} {m.M33:+0.0000;-0.0000} {m.M34:+0.0000;-0.0000}");
            Logger.Write($"{m.M41:+0.0000;-0.0000} {m.M42:+0.0000;-0.0000} {m.M43:+0.0000;-0.0000} {m.M44:+0.0000;-0.0000}");
        }
    }
}
