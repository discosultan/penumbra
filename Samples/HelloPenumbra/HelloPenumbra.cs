using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Penumbra;
using System;

namespace HelloPenumbra
{    
    public class HelloPenumbra : Game
    {
        GraphicsDeviceManager graphics;        

        // Store reference to lighting system.
        PenumbraComponent penumbra;

        // Create sample light source and shadow hull.
        Light light = new PointLight
        {            
            Scale = new Vector2(1000f), // Range of the light source (how far the light will travel)
            ShadowType = ShadowType.Solid // Will not lit hulls themselves
        };
        Hull hull = new Hull(new Vector2(1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f), new Vector2(1.0f, -1.0f))
        {
            Position = new Vector2(400f, 240f),
            Scale = new Vector2(50f)
        };

        public HelloPenumbra()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Create the lighting system and add sample light and hull.
            penumbra = new PenumbraComponent(this);            
            penumbra.Lights.Add(light);
            penumbra.Hulls.Add(hull);
        }

        protected override void Initialize()
        {
            // Initialize the lighting system.
            penumbra.Initialize();            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Animate light position and hull rotation.
            light.Position = 
                new Vector2(400f, 240f) + // Offset origin
                new Vector2( // Position around origin
                    (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds),
                    (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds)) * 240f;
            hull.Rotation = MathHelper.WrapAngle(-(float)gameTime.TotalGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Everything between penumbra.BeginDraw and penumbra.Draw will be
            // lit by the lighting system.
            penumbra.BeginDraw();

            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw items affected by lighting here ...

            penumbra.Draw(gameTime);

            // Draw items NOT affected by lighting here ... (UI, for example)

            base.Draw(gameTime);
        }
    }
}
