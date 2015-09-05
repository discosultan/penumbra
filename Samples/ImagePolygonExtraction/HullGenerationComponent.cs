using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Penumbra;
using Penumbra.Core;

namespace ImagePolygonExtraction
{
    class HullGenerationComponent : GameComponent
    {
        private readonly PenumbraComponent _penumbra;

        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        private Hull _activeHull;

        public HullGenerationComponent(Game game, PenumbraComponent penumbra) : base(game)
        {
            _penumbra = penumbra;
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            MouseState currentMouseState = Mouse.GetState();

            if (!_previousKeyboardState.IsKeyDown(Keys.Space) && 
                currentKeyboardState.IsKeyDown(Keys.Space))
            {
                if (_activeHull != null && _activeHull.Points.Count > 0)
                {
                    _activeHull.Points.RemoveAt(_activeHull.Points.Count - 1);
                }
            }

            if (_previousMouseState.LeftButton == ButtonState.Released &&
                currentMouseState.LeftButton == ButtonState.Pressed)
            {
                if (_activeHull == null)
                {
                    _activeHull = new Hull();
                    _penumbra.Hulls.Add(_activeHull);
                }
                _activeHull.Points.Add(currentMouseState.Position.ToVector2());
            }

            if (_previousMouseState.RightButton == ButtonState.Released &&
                currentMouseState.RightButton == ButtonState.Pressed)
            {
                _activeHull = null;
            }

            _previousKeyboardState = currentKeyboardState;
            _previousMouseState = currentMouseState;
        }
    }
}
