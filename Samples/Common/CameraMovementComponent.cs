using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Penumbra;

namespace Common
{
    public class CameraMovementComponent : GameComponent
    {
        private const Keys Up = Keys.W;
        private const Keys Down = Keys.S;
        private const Keys Left = Keys.A;
        private const Keys Right = Keys.D;
        private const Keys ZoomIn = Keys.R;
        private const Keys ZoomOut = Keys.F;
        private const Keys RotateLeft = Keys.Q;
        private const Keys RotateRight = Keys.E;

        private readonly PenumbraComponent _penumbra;

        private bool _dirty = true;
        private Vector2 _position;
        private float _scale = 1.0f;
        private float _rotation;
        private Vector2 _origin;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; _dirty = true; }
        }

        public float Scale
        {
            get { return _scale; }
            set { _scale = value; _dirty = true; }
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; _dirty = true; }
        }

        public Vector2 Origin
        {
            get { return _origin; }
            set { _origin = value; _dirty = true; }
        }

        public float MoveSpeed { get; set; } = 600f;
        public float ZoomSpeed { get; set; } = 0.75f;
        public float MinZoom { get; set; } = 0.1f;
        public float MaxZoom { get; set; } = 3.0f;
        public float RotationSpeed { get; set; } = MathHelper.TwoPi * 0.25f;
        public bool InvertedY { get; set; }

        private Matrix _transform;

        public Matrix Transform
        {
            get { return _transform; }
            set
            {
                _transform = value;
                _penumbra.Transform = value;
                InverseTransform = Matrix.Invert(value);
            }
        }

        public Matrix InverseTransform { get; private set; }

        public bool InputEnabled { get; set; } = true;

        public CameraMovementComponent(Game game, PenumbraComponent penumbra) : base(game)
        {
            _penumbra = penumbra;

            Enabled = true;
        }        

        public override void Update(GameTime gameTime)
        {
            if (InputEnabled)
            {
                KeyboardState keyState = Keyboard.GetState();
                var deltaSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;

                Vector2 moveDirection = Vector2.Zero;
                float zoomDir = 0;
                float rotationDir = 0;

                if (keyState.IsKeyDown(Left))
                    moveDirection.X -= 1f;
                if (keyState.IsKeyDown(Right))
                    moveDirection.X += 1f;
                if (InvertedY && keyState.IsKeyDown(Down) || !InvertedY && keyState.IsKeyDown(Up))
                    moveDirection.Y += 1f;
                if (InvertedY && keyState.IsKeyDown(Up) || !InvertedY && keyState.IsKeyDown(Down))
                    moveDirection.Y -= 1f;
                if (keyState.IsKeyDown(ZoomIn))
                    zoomDir += 1;
                if (keyState.IsKeyDown(ZoomOut))
                    zoomDir -= 1;
                if (keyState.IsKeyDown(RotateLeft))
                    rotationDir -= 1;
                if (keyState.IsKeyDown(RotateRight))
                    rotationDir += 1;

                if (moveDirection != Vector2.Zero)
                {
                    moveDirection.Normalize();
                    moveDirection = Vector2.TransformNormal(moveDirection, Matrix.CreateRotationZ(-Rotation));
                    Position += moveDirection*MoveSpeed*deltaSeconds;
                    _dirty = true;
                }
                if (zoomDir != 0)
                {
                    Scale = MathHelper.Clamp(Scale + zoomDir*ZoomSpeed*deltaSeconds, MinZoom, MaxZoom);
                    _dirty = true;
                }
                if (rotationDir != 0)
                {
                    Rotation = MathHelper.WrapAngle(Rotation + rotationDir*RotationSpeed*deltaSeconds);
                    _dirty = true;
                }
            }

            if (_dirty)
                SetViewMatrix();
        }

        private void SetViewMatrix()
        {
            Transform =
                Matrix.CreateTranslation(new Vector3(-Position, 0)) *
                Matrix.CreateRotationZ(Rotation) *                
                Matrix.CreateScale(Scale) *
                Matrix.CreateTranslation(new Vector3(Origin, 0));
        }
    }
}
