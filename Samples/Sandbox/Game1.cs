using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Penumbra;

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
        public const Keys DebugKey = Keys.D;
        public const Keys ShadowTypeKey = Keys.S;

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
            _penumbra = new PenumbraComponent(this);            
            Components.Add(_penumbra);
            _scenarios = new ScenariosComponent(this, _penumbra);
            Components.Add(_scenarios);
            var ui = new UIComponent(this) { DrawOrder = int.MaxValue };
            Components.Add(ui);
            
            IsMouseVisible = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {            
            _penumbra.ViewProjection = Matrix.CreateOrthographicOffCenter(
                -GraphicsDevice.PresentationParameters.BackBufferWidth/2f,
                GraphicsDevice.PresentationParameters.BackBufferWidth/2f,
                -GraphicsDevice.PresentationParameters.BackBufferHeight/2f,
                GraphicsDevice.PresentationParameters.BackBufferHeight/2f,
                0f, 1f);
            // TODO: use this.Content to load your game content here
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
                _penumbra.DebugDraw = !_penumbra.DebugDraw;

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

            GraphicsDevice.Clear(Color.CornflowerBlue);            

            base.Draw(gameTime);
        }

        private bool IsKeyPressed(Keys key)
        {
            return !_previousKeyState.IsKeyDown(key) && _currentKeyState.IsKeyDown(key);
        }
    }
}
