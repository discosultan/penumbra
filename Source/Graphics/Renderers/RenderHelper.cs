using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics.Renderers
{
    internal class RenderHelper : IDisposable
    {
        private PenumbraEngine _engine;
        private SpriteBatch _spriteBatch;

        public void Initialize(PenumbraEngine engine)
        {
            _engine = engine;
            _spriteBatch = new SpriteBatch(_engine.GraphicsDevice);
        }

        public void Dispose()
        {
            _spriteBatch?.Dispose();
        }

        public void Render(Texture2D texture, Rectangle destination)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            _spriteBatch.Draw(texture, destination, Color.White);
            _spriteBatch.End();
        }
    }
}
