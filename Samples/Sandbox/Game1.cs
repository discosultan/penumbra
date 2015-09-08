using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Penumbra;
using Penumbra.Core;

namespace Sandbox
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public const Keys PreviousScenarioKey = Keys.Left;
        public const Keys NextScenarioKey = Keys.Right;
        public const Keys PauseKey = Keys.Space;
        //public const Keys DebugKey = Keys.D;
        //public const Keys ShadowTypeKey = Keys.S;
        public const Keys DebugKey = Keys.T;
        public const Keys ShadowTypeKey = Keys.G;

        private static readonly Color BackgroundColor = Color.White;

        private readonly GraphicsDeviceManager _graphics;        

        private readonly PenumbraComponent _penumbra;
        private readonly ScenariosComponent _scenarios;        

        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;

        internal ScenariosComponent Scenarios => _scenarios;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _penumbra = new PenumbraComponent(this, Projections.OriginCenter_XRight_YUp | Projections.Custom)
            {
                AmbientColor = Color.Black
            };
            Components.Add(_penumbra);
            _scenarios = new ScenariosComponent(this, _penumbra);
            Components.Add(_scenarios);
            var ui = new UIComponent(this)
            {
                DrawOrder = int.MaxValue
            };
            Components.Add(ui);
            Components.Add(new CameraMovementComponent(this, _penumbra));
            
            IsMouseVisible = true;
            // Disable FPS limit.
            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = false;            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            _penumbra.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _currentKeyState = Keyboard.GetState();

            if (IsKeyPressed(PauseKey))
                _scenarios.Enabled = !_scenarios.Enabled;

            if (IsKeyPressed(NextScenarioKey))
                _scenarios.NextScenario();

            if (IsKeyPressed(PreviousScenarioKey))
                _scenarios.PreviousScenario();

            if (IsKeyPressed(DebugKey))
                _penumbra.Debug = !_penumbra.Debug;

            if (IsKeyPressed(ShadowTypeKey))
                _scenarios.NextShadowType();

            _previousKeyState = _currentKeyState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _penumbra.BeginDraw();

            GraphicsDevice.Clear(BackgroundColor);            

            base.Draw(gameTime);
        }

        private bool IsKeyPressed(Keys key)
        {
            return !_previousKeyState.IsKeyDown(key) && _currentKeyState.IsKeyDown(key);
        }
    }
}
