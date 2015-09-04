using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Penumbra;

namespace ImagePolygonExtraction
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _logo;
        private readonly PenumbraComponent _penumbra;
        private Light _light;      

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 768
            };

            Content.RootDirectory = "Content";

            _penumbra = new PenumbraComponent(this)
            {
                AmbientColor = Color.Gray                
            };
            Components.Add(_penumbra);
            Components.Add(new HullGenerationComponent(this, _penumbra));            
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _logo = Content.Load<Texture2D>("ttu_logo");

            _light = new Light
            {
                Position = Vector2.Zero,
                Range = 500
            };
            _penumbra.Lights.Add(_light);
            
            //penumbra.Hulls.Add(new Hull(result[0], WindingOrder.CounterClockwise));
            //_penumbra.Hulls.Add(
            //    new Hull(new[]
            //    {
            //        new Vector2(100, 100), new Vector2(200, 100), new Vector2(200, 200), new Vector2(100, 200)
            //    }));
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            _penumbra.Dispose();
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _currentKeyState = Keyboard.GetState();

            var ms = Mouse.GetState();
            _light.Position = ms.Position.ToVector2();

            _previousKeyState = _currentKeyState;

            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _penumbra.BeginDraw();

            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_logo, Vector2.Zero, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private bool IsKeyPressed(Keys key)
        {
            return !_previousKeyState.IsKeyDown(key) && _currentKeyState.IsKeyDown(key);
        }
    }
}
