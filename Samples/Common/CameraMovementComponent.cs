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

        private Vector2 _position;
        
        private readonly PenumbraComponent _penumbra;

        public float MoveSpeed { get; set; } = 600f;
        public bool InvertedY { get; set; }

        public CameraMovementComponent(Game game, PenumbraComponent penumbra) : base(game)
        {
            _penumbra = penumbra;
            Enabled = true;
        }        

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            Vector2 moveDirection = Vector2.Zero;
            if (keyState.IsKeyDown(Left))
                moveDirection.X -= 1f;
            if (keyState.IsKeyDown(Right))
                moveDirection.X += 1f;
            if (InvertedY && keyState.IsKeyDown(Down) || !InvertedY && keyState.IsKeyDown(Up))
                moveDirection.Y += 1f;
            if (InvertedY && keyState.IsKeyDown(Up) || !InvertedY && keyState.IsKeyDown(Down))
                moveDirection.Y -= 1f;

            if (moveDirection != Vector2.Zero)
            {
                moveDirection.Normalize();
                _position += moveDirection * MoveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                SetViewMatrix();
            }
        }

        private void SetViewMatrix()
        {
            _penumbra.ViewProjection = Matrix.CreateTranslation(new Vector3(-_position, 0));
        }
    }
}
