#region File Description

//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.IO;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Penumbra;
using Platformer2D.Game;
using QuakeConsole;

namespace Platformer2D
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        private const Keys ConsoleToggleOpenKey = Keys.OemTilde;

        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 3;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(15);

        // Resources for drawing.
        private readonly GraphicsDeviceManager graphics;
        private readonly PenumbraControllerComponent penumbraController;

        private readonly PointLight pointLight = new PointLight { Scale = new Vector2(800), Color = Color.White };

        private readonly Spotlight spotlight = new Spotlight
        {
            Scale = new Vector2(1000, 1200),
            Color = new Color(150, 255, 150, 255),
            Intensity = 1f,
            ConeDecay = 1.5f
        };

        private TexturedLight texturedLight;

        private AccelerometerState accelerometerState;
        private Vector2 baseScreenSize = new Vector2(800, 480);
        private Texture2D diedOverlay;

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private Matrix globalTransformation;

        // Global content.
        private SpriteFont consoleFont;
        private SpriteFont hudFont;
        private KeyboardState keyboardState;
        private Level level;

        // Meta-level game state.
        private int levelIndex = -1;
        private Texture2D loseOverlay;
        private KeyboardState previousKeyboardState;
        private SpriteBatch spriteBatch;
        private TouchCollection touchState;

        private VirtualGamePad virtualGamePad;
        private bool wasContinuePressed;

        private Texture2D winOverlay;

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            IsMouseVisible = true;

            Accelerometer.Initialize();

            Penumbra = new PenumbraComponent(this)
            {
                AmbientColor = new Color(30, 20, 10),
                Visible = true,
                Debug = false                
            };

            penumbraController = new PenumbraControllerComponent(this, Penumbra);
            Components.Add(penumbraController);

            Console = new ConsoleComponent(this);
            Console.ActionMappings.Remove(ConsoleAction.Tab);
            Console.ActionMappings.Remove(ConsoleAction.RemoveTab);
            Console.ActionMappings.Add(Keys.Tab, ConsoleAction.AutocompleteForward);
            Console.ActionMappings.Add(Keys.LeftShift, Keys.Tab, ConsoleAction.AutocompleteBackward);
            Console.ActionMappings.Add(Keys.RightShift, Keys.Tab, ConsoleAction.AutocompleteBackward);
            Components.Add(Console);            
        }

        public PenumbraComponent Penumbra { get; }
        public ConsoleComponent Console { get; }
        public PythonInterpreter Interpreter { get; } = new PythonInterpreter();

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            consoleFont = Content.Load<SpriteFont>("Fonts/Console");
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");

            Penumbra.Initialize();
            Console.LoadContent(consoleFont, Interpreter);
            texturedLight = new TexturedLight(Content.Load<Texture2D>("Sprites/Light"))
            {
                Origin = new Vector2(0.5f, 0.5f)
            };

            //Work out how much we need to scale our graphics to fill the screen
            float horScaling = GraphicsDevice.PresentationParameters.BackBufferWidth / baseScreenSize.X;
            float verScaling = GraphicsDevice.PresentationParameters.BackBufferHeight / baseScreenSize.Y;
            var screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            globalTransformation = Matrix.CreateScale(screenScalingFactor);
            Penumbra.Transform = globalTransformation;

            virtualGamePad = new VirtualGamePad(baseScreenSize, globalTransformation,
                Content.Load<Texture2D>("Sprites/VirtualControlArrow"));

            //Known issue that you get exceptions if you use Media PLayer while connected to your PC
            //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
            //Which means its impossible to test this from VS.
            //So we have to catch the exception and throw it away
            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
            }
            catch
            {
            }

            Interpreter.AddVariable("penumbra", Penumbra);
            Interpreter.AddVariable("spotlight", spotlight);
            Interpreter.AddVariable("pointLight", pointLight);
            Interpreter.AddVariable("texturedLight", texturedLight);

            LoadNextLevel();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Handle polling for our input and handling high-level input            
            HandleInput(gameTime);

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime, keyboardState, gamePadState,
                accelerometerState, Window.CurrentOrientation);

            if (level.Player.Velocity != Vector2.Zero)
                virtualGamePad.NotifyPlayerIsMoving();

            texturedLight.Rotation = MathHelper.WrapAngle((float)gameTime.TotalGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            // get all of our input states
            previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            touchState = TouchPanel.GetState();
            gamePadState = virtualGamePad.GetState(touchState, GamePad.GetState(PlayerIndex.One));
            accelerometerState = Accelerometer.GetState();
            MouseState mouseState = Mouse.GetState();

            Vector2 playerPos = Vector2.Transform(level.Player.Position, globalTransformation);
            Vector2 lookDir = Vector2.Normalize(mouseState.Position.ToVector2() - playerPos);
            spotlight.Rotation = (float) Math.Atan2(lookDir.Y, lookDir.X);            

#if !NETFX_CORE
            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();
#endif
            if (keyboardState.IsKeyDown(ConsoleToggleOpenKey) && !previousKeyboardState.IsKeyDown(ConsoleToggleOpenKey))
                Console.ToggleOpenClose();

            var continuePressed = false;
            if (!Console.IsAcceptingInput)
            {
                penumbraController.InputEnabled = true;

                continuePressed =
                    keyboardState.IsKeyDown(Keys.Space) ||
                    gamePadState.IsButtonDown(Buttons.A) ||
                    touchState.AnyTouch();

                // Perform the appropriate action to advance the game and
                // to get the player back to playing.
                if (!wasContinuePressed && continuePressed)
                {
                    if (!level.Player.IsAlive)
                    {
                        level.StartNewLife();
                    }
                    else if (level.TimeRemaining == TimeSpan.Zero)
                    {
                        if (level.ReachedExit)
                            LoadNextLevel();
                        else
                            ReloadCurrentLevel();
                    }
                }
            }
            else
            {
                penumbraController.InputEnabled = false;
            }
            wasContinuePressed = continuePressed;

            virtualGamePad.Update(gameTime);
        }

        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1) % numberOfLevels;

            // Unloads the content for the current level before loading the next one.
            level?.Dispose();            

            // Load the level.
            string levelPath = $"Content/Levels/{levelIndex}.txt";
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex, this);            
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Penumbra.BeginDraw();

            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, transformMatrix: globalTransformation);
            level.Draw(gameTime, spriteBatch);
            spriteBatch.End();

            Penumbra.Draw(gameTime);

            spriteBatch.Begin(SpriteSortMode.Deferred);
            DrawHud();
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            var hudLocation = new Vector2(titleSafeArea.X + titleSafeArea.Width, titleSafeArea.Y);
            //var center = new Vector2(baseScreenSize.X / 2, baseScreenSize.Y / 2);
            var center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2, titleSafeArea.Y + titleSafeArea.Height / 2);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" +
                                level.TimeRemaining.Seconds.ToString("00");
            string scoreString = "SCORE: " + level.Score;
            Vector2 timeSize = hudFont.MeasureString(timeString);
            Vector2 scoreSize = hudFont.MeasureString(scoreString);
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int) level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }            
            DrawShadowedString(hudFont, timeString, hudLocation + new Vector2(-timeSize.X, 0), timeColor);

            // Draw score            
            DrawShadowedString(hudFont, scoreString, hudLocation + new Vector2(-scoreSize.X, timeSize.Y * 1.2f),
                Color.Yellow);

            // Draw info/instructions
            string infoString = $"Lighting enabled: {Penumbra.Visible} ({penumbraController.EnabledKey})\n" +
                                $"Shadow type: {penumbraController.ActiveShadowType} ({penumbraController.ShadowTypeKey})\n" +
                                $"Debug mode: {Penumbra.Debug} ({penumbraController.DebugKey})\n" +
                                $"Console open: {Console.IsVisible} ({ConsoleToggleOpenKey})";
            Vector2 infoSize = consoleFont.MeasureString(infoString);
            DrawShadowedString(
                consoleFont, 
                infoString, 
                new Vector2(titleSafeArea.X, titleSafeArea.Y + titleSafeArea.Height - infoSize.Y) + new Vector2(10, -10), 
                Color.Yellow);

            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                status = level.ReachedExit ? winOverlay : loseOverlay;
            }
            else if (!level.Player.IsAlive)
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                var statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            //if (touchState.IsConnected)
            //    virtualGamePad.Draw(spriteBatch);
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}