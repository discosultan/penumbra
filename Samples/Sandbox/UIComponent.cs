using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox
{
    class UIComponent : DrawableGameComponent
    {
        private const float Padding = 5f;
        private static readonly Color Color = Color.Yellow;

        private SpriteBatch _spriteBatch;
        private SpriteFont _font;
        private Vector2 _fontSize;        

        private string _info;
        private Vector2 _infoPosition;

        private string _shadowType;        

        private readonly ScenariosComponent _scenarios;

        public UIComponent(Game1 game) : base(game)
        {
            _scenarios = game.Scenarios;
                   
            _scenarios.ShadowTypeChanged += (s, e) => { _shadowType = $"Shadow type: {_scenarios.ActiveShadowType}"; };

            Enabled = false;
            Visible = true;
        }

        protected override void LoadContent()
        {            
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Game.Content.Load<SpriteFont>("Font");
            _fontSize = _font.MeasureString("I");

            _info = $"Previous {Game1.PreviousScenarioKey} | Next {Game1.NextScenarioKey} | Pause {Game1.PauseKey} | Debug {Game1.DebugKey} | Shadow type {Game1.ShadowTypeKey}";
            Vector2 size = _font.MeasureString(_info);
            _infoPosition = new Vector2(Padding, GraphicsDevice.Viewport.Height - size.Y - Padding);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();            
            _spriteBatch.DrawString(_font, _scenarios.ActiveScenario.Name, new Vector2(Padding), Color);
            _spriteBatch.DrawString(_font, _shadowType, new Vector2(Padding, Padding * 2 + _fontSize.Y), Color);
            _spriteBatch.DrawString(_font, _info, _infoPosition, Color);            
            _spriteBatch.End();
        }
    }
}
