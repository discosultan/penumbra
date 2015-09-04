using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Penumbra
{
    public class PenumbraComponent : DrawableGameComponent
    {        
        private readonly PenumbraEngine _engine;

        public PenumbraComponent(Game game, Projections projections = Projections.SpriteBatch | Projections.Custom) 
            : base(game)
        {
            _engine = new PenumbraEngine(projections);
            Enabled = false;
            Visible = true;
        }

        public bool DebugDraw
        {
            get { return _engine.DebugDraw; }
            set { _engine.DebugDraw = value; }
        }

        public Color AmbientColor
        {
            get { return _engine.AmbientColor; }
            set { _engine.AmbientColor = value; }
        }

        public Matrix ViewProjection
        {
            get { return _engine.ViewProjection; }
            set { _engine.ViewProjection = value; }
        }

        public IList<Light> Lights => _engine.Lights;
        public IList<Hull> Hulls => _engine.Hulls;

        protected override void LoadContent()
        {            
            var graphicsDeviceManager = (GraphicsDeviceManager)Game.Services.GetService<IGraphicsDeviceManager>();
            _engine.Load(GraphicsDevice, graphicsDeviceManager, Game.Content);            
        }

        public void BeginDraw()
        {
            _engine.PreRender();
        }

        public override void Draw(GameTime gameTime)
        {
            _engine.Render();
        }
    }
}
