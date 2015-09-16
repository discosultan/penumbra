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

        private string _shadowTypeInfo = "";
        private string _info;
        private Vector2 _infoPosition;

        private readonly ScenariosComponent _scenarios;

        public UIComponent(SandboxGame game, PenumbraControllerComponent penumbraController) : base(game)
        {
            _penumbraController = penumbraController;
            _scenarios = game.Scenarios;

            penumbraController.ShadowTypeChanged += (s, e) => SetShadowTypeInfo();            
            SetShadowTypeInfo();

            Enabled = false;
            Visible = true;
        }

        private void SetShadowTypeInfo()
        {
            _shadowTypeInfo = $"Shadow type: {_penumbraController.ActiveShadowType}";
        }

        protected override void LoadContent()
        {            
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Game.Content.Load<SpriteFont>("Font");
            _fontSize = _font.MeasureString("I");

            _info = $"Previous {Sandbox.SandboxGame.PreviousScenarioKey} | Next {Sandbox.SandboxGame.NextScenarioKey} | Pause {Sandbox.SandboxGame.PauseKey} | Debug {PenumbraControllerComponent.DebugKey} | Shadow type {PenumbraControllerComponent.ShadowTypeKey}";
            Vector2 size = _font.MeasureString(_info);
            _infoPosition = new Vector2(Padding, GraphicsDevice.Viewport.Height - size.Y - Padding);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();            
            _spriteBatch.DrawString(_font, _scenarios.ActiveScenario.Name, new Vector2(Padding), Color);
            _spriteBatch.DrawString(_font, _shadowTypeInfo, new Vector2(Padding, Padding * 2 + _fontSize.Y), Color);
            _spriteBatch.DrawString(_font, _info, _infoPosition, Color);            
            _spriteBatch.End();
        }
    }
}
