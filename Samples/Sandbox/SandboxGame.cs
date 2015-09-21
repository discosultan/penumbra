using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Penumbra;

namespace Sandbox
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class SandboxGame : Game
    {
        public const Keys PreviousScenarioKey = Keys.Left;
        public const Keys NextScenarioKey = Keys.Right;
        public const Keys PauseKey = Keys.Space;

        private static readonly Color BackgroundColor = Color.White;

        private readonly GraphicsDeviceManager _deviceManager;
        private readonly PenumbraComponent _penumbra;
        private readonly ScenariosComponent _scenarios;

        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;

        internal ScenariosComponent Scenarios => _scenarios;

        public SandboxGame()
        {            
            _deviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _penumbra = new PenumbraComponent(this, Projections.OriginCenter_XRight_YUp | Projections.Custom)
            {
                AmbientColor = Color.Black
            };
            Components.Add(_penumbra);            
            var penumbraController = new PenumbraControllerComponent(this, _penumbra);
            Components.Add(penumbraController);
            _scenarios = new ScenariosComponent(this, _penumbra, penumbraController);
            Components.Add(_scenarios);
            var ui = new UIComponent(this, penumbraController)
            {
                DrawOrder = int.MaxValue
            };
            Components.Add(ui);
            Components.Add(new CameraMovementComponent(this, _penumbra));
            Components.Add(new FpsGarbageComponent(this));

            // There's a bug when trying to change resolution during window resize.
            // https://github.com/mono/MonoGame/issues/3572
            _deviceManager.PreferredBackBufferWidth = 1240;
            _deviceManager.PreferredBackBufferHeight = 768;
            Window.AllowUserResizing = false;            
            IsMouseVisible = true;            
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
