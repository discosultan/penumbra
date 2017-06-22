using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sandbox
{
    class UIComponent : DrawableGameComponent
    {
        private static readonly Color ForegroundColorDefault = Color.White;
        private static readonly Color ForegroundColorHighlight = Color.Yellow;

        private SpriteBatch _spriteBatch;
        private SpriteFont _font;

        public UIComponent(SandboxGame game, PenumbraControllerComponent penumbraController) : base(game)
        {
            PenumbraController = penumbraController;
            Scenarios = game.Scenarios;

            Enabled = false;
            Visible = true;
        }

        public PenumbraControllerComponent PenumbraController { get; }
        public ScenariosComponent Scenarios { get; }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Game.Content.Load<SpriteFont>("Font");
        }

        public override void Draw(GameTime gameTime)
        {
            const float margin = 10;
            float sizeY = _font.LineSpacing;

            _spriteBatch.Begin();

            // Draw key instructions.
            float progressX = margin;
            float progressY = GraphicsDevice.Viewport.Height - margin - sizeY;
            for (int i = _instructionKeys.Length - 1; i >= 0; i--)
            {
                _spriteBatch.DrawString(_font, _instructionPrefixes[i], new Vector2(progressX, progressY), ForegroundColorDefault);
                progressX += _font.MeasureString(_instructionPrefixes[i]).X;
                _spriteBatch.DrawString(_font, _instructionKeys[i], new Vector2(progressX, progressY), ForegroundColorHighlight);
                progressX += _font.MeasureString(_instructionKeys[i]).X;
                _spriteBatch.DrawString(_font, _instructionSuffixes[i], new Vector2(progressX, progressY), ForegroundColorDefault);

                progressY -= sizeY;
                progressX = margin;
            }

            // Draw current setup info.
            progressY = GraphicsDevice.Viewport.Height - margin - sizeY;
            for (int i = _infos.Length - 1; i >= 0; i--)
            {
                string info = _infos[i](this);
                progressX = GraphicsDevice.Viewport.Width - margin - _font.MeasureString(info).X;
                _spriteBatch.DrawString(_font, info, new Vector2(progressX, progressY), ForegroundColorHighlight);
                progressX -= _font.MeasureString(_infoPrefixes[i]).X;
                _spriteBatch.DrawString(_font, _infoPrefixes[i], new Vector2(progressX, progressY), ForegroundColorDefault);

                progressY -= sizeY;
            }

            _spriteBatch.End();
        }

        private string[] _instructionPrefixes =
        {
            "Previous scenario [ ",
            "Next scenario [ ",
            "Change shadow type [ ",
            "Show debug lines [ ",
            "Pause/resume simulation [ "
        };
        private string[] _instructionKeys =
        {
            SandboxGame.PreviousScenarioKey.ToString(),
            SandboxGame.NextScenarioKey.ToString(),
            PenumbraControllerComponent.ShadowTypeKey.ToString(),
            PenumbraControllerComponent.DebugKey.ToString(),
            SandboxGame.PauseKey.ToString()
        };
        private string[] _instructionSuffixes = { " ]", " ]", " ]", " ]", " ]" };

        private string[] _infoPrefixes =
        {
            "Scenario: ",
            "Shadow Type: "
        };
        private Func<UIComponent, string>[] _infos =
        {
            ui => ui.Scenarios.ActiveScenario.Name,
            ui => ui.PenumbraController.ActiveShadowType.ToString()
        };
    }
}
