using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox
{
    class UIComponent : DrawableGameComponent
    {        
        private const float Padding = 5f;
        private static readonly Color Color = Color.Yellow;

        private readonly PenumbraControllerComponent _penumbraController;

        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Vector2 _fontSize;        

        private string _info;
        private Vector2 _infoPosition;

        private readonly ScenariosComponent _scenarios;

        public UIComponent(Game1 game, PenumbraControllerComponent penumbraController) : base(game)
        {
            _penumbraController = penumbraController;
            _scenarios = game.Scenarios;                               

            Enabled = false;
            Visible = true;
        }

        protected override void LoadContent()
        {            
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Game.Content.Load<SpriteFont>("Font");
            _fontSize = _font.MeasureString("I");

            _info = $"Previous {Game1.PreviousScenarioKey} | Next {Game1.NextScenarioKey} | Pause {Game1.PauseKey} | Debug {PenumbraControllerComponent.DebugKey} | Shadow type {PenumbraControllerComponent.ShadowTypeKey}";
            Vector2 size = _font.MeasureString(_info);
            _infoPosition = new Vector2(Padding, GraphicsDevice.Viewport.Height - size.Y - Padding);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();            
            _spriteBatch.DrawString(_font, _scenarios.ActiveScenario.Name, new Vector2(Padding), Color);
            _spriteBatch.DrawString(_font, $"Shadow type: {_penumbraController.ActiveShadowType}", new Vector2(Padding, Padding * 2 + _fontSize.Y), Color);
            _spriteBatch.DrawString(_font, _info, _infoPosition, Color);            
            _spriteBatch.End();
        }
    }
}
