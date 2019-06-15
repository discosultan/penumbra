using Microsoft.Xna.Framework;
using Penumbra.Geometry;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Providers
{
    internal class CameraProvider : GraphicsProvider
    {
        public Matrix ViewProjection = Matrix.Identity;
        public BoundingRectangle Bounds = new BoundingRectangle(new Vector2(float.MinValue), new Vector2(float.MaxValue));

        private Matrix _inverseViewProjection = Matrix.Identity;
        private Matrix _clipToScreen = Matrix.Identity;
        private Matrix _spriteBatchTransform = Matrix.Identity;
        private Matrix _custom = Matrix.Identity;
        private bool _loaded;

        public bool InvertedY { get; private set; }
        public bool SpriteBatchTransformEnabled { get; set; } = true;

        public Matrix Custom
        {
            get { return _custom; }
            set
            {
                _custom = value;
                if (_loaded)
                    CalculateViewProjectionAndBounds();
            }
        }

        public override void Load(PenumbraEngine engine)
        {
            base.Load(engine);

            CalculateSpriteBatchTransform();
            CalculateClipToScreen();
            CalculateViewProjectionAndBounds();

            _loaded = true;
        }

        // Calculates a screen space rectangle based on world space bounds.
        public void GetScissorRectangle(Light light, out BoundingRectangle scissor)
        {
            Matrix.Multiply(ref ViewProjection, ref _clipToScreen, out Matrix transform);

            BoundingRectangle.Transform(ref light.Bounds, ref transform, out scissor);
        }

        protected override void OnSizeChanged()
        {
            Logger.Write($"Screen size changed to {ViewportWidth}x{ViewportHeight}.");
            CalculateSpriteBatchTransform();
            CalculateClipToScreen();
            CalculateViewProjectionAndBounds();
        }

        private void CalculateSpriteBatchTransform()
        {
            Matrix.CreateOrthographicOffCenter(
                0,
                ViewportWidth,
                ViewportHeight,
                0,
                0.0f, 1.0f, out _spriteBatchTransform);
        }

        private void CalculateClipToScreen()
        {
            _clipToScreen = Matrix.Invert(Matrix.CreateOrthographicOffCenter(0, ViewportWidth, ViewportHeight, 0, 0, 1));
        }

        private void CalculateViewProjectionAndBounds()
        {
            if (SpriteBatchTransformEnabled)
                Matrix.Multiply(ref _custom, ref _spriteBatchTransform, out ViewProjection);
            else
                ViewProjection = _custom;

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