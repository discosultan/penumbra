using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Penumbra;
using QuakeConsole;

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

        private readonly PenumbraControllerComponent _penumbraController;
        private readonly PenumbraComponent _penumbra;
        private readonly CameraMovementComponent _camera;
        private readonly ConsoleComponent _console;
        private readonly PythonInterpreter _consoleInterpreter = new PythonInterpreter();

        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;

        private Matrix _projection;

        internal ScenariosComponent Scenarios { get; }

        public SandboxGame()
        {
            var deviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";            
            _penumbra = new PenumbraComponent(this)
            {
                SpriteBatchTransformEnabled = false,
                AmbientColor = Color.Black
            };
            Components.Add(_penumbra);
            _penumbraController = new PenumbraControllerComponent(this, _penumbra);
            Components.Add(_penumbraController);
            Scenarios = new ScenariosComponent(this, _penumbra, _penumbraController, _consoleInterpreter);
            Components.Add(Scenarios);
            var ui = new UIComponent(this, _penumbraController)
            {
                DrawOrder = int.MaxValue
            };
            Components.Add(ui);
            _camera = new CameraMovementComponent(this);
            Components.Add(_camera);
            Components.Add(new FpsGarbageComponent(this));
            _console = new ConsoleComponent(this);
            Components.Add(_console);

            // There's a bug when trying to change resolution during window resize.
            // https://github.com/mono/MonoGame/issues/3572
            deviceManager.PreferredBackBufferWidth = 1280;
            deviceManager.PreferredBackBufferHeight = 720;
            Window.AllowUserResizing = false;            
            IsMouseVisible = true;            
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            var pp = GraphicsDevice.PresentationParameters;
            _projection = Matrix.CreateOrthographicOffCenter(
                -pp.BackBufferWidth / 2.0f,
                pp.BackBufferWidth / 2.0f,
                -pp.BackBufferHeight / 2.0f,
                pp.BackBufferHeight / 2.0f,
                0.0f, 1.0f);

            _console.LoadContent(Content.Load<SpriteFont>("Font"), _consoleInterpreter);
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

            _penumbraController.InputEnabled = !_console.IsAcceptingInput;
            _camera.InputEnabled = !_console.IsAcceptingInput;
            if (!_console.IsAcceptingInput)
            {
                if (IsKeyPressed(PauseKey))
                    Scenarios.Enabled = !Scenarios.Enabled;

                if (IsKeyPressed(NextScenarioKey))
                    Scenarios.NextScenario();

                if (IsKeyPressed(PreviousScenarioKey))
                    Scenarios.PreviousScenario();
            }

            if (IsKeyPressed(Keys.OemTilde))
                _console.ToggleOpenClose();

            _previousKeyState = _currentKeyState;

            // View * projection.
            _penumbra.Transform = _camera.Transform * _projection;

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
