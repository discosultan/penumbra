using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Penumbra.Core;

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

        private Vector2 _position;
        private float _scale = 1.0f;
        private float _rotation;                

        public float MoveSpeed { get; set; } = 600f;
        public float ZoomSpeed { get; set; } = 0.65f;
        public float MinZoom { get; set; } = 0.1f;
        public float MaxZoom { get; set; } = 3.0f;
        public float RotationSpeed { get; set; } = 1 / MathHelper.TwoPi * 4;
        public bool InvertedY { get; set; }

        public CameraMovementComponent(Game game, PenumbraComponent penumbra) : base(game)
        {
            _penumbra = penumbra;

            Enabled = true;
        }        

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
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
                _position += moveDirection * MoveSpeed * deltaSeconds;
            }
            if (zoomDir != 0)
                _scale = MathHelper.Clamp(_scale + zoomDir * ZoomSpeed * deltaSeconds, MinZoom, MaxZoom);
            if (rotationDir != 0)
                _rotation = MathHelper.WrapAngle(_rotation + rotationDir * RotationSpeed * deltaSeconds);

            if (moveDirection != Vector2.Zero || zoomDir != 0 || rotationDir != 0)
                SetViewMatrix();
        }

        private void SetViewMatrix()
        {
            _penumbra.Transform =
				Matrix.CreateScale(_scale);
                * Matrix.CreateRotationZ(_rotation)
                * Matrix.CreateTranslation(new Vector3(-_position, 0));
        }
    }
}
