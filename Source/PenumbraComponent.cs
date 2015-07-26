using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Penumbra
{
    public class PenumbraComponent : DrawableGameComponent
    {        
        private readonly PenumbraEngine _engine = new PenumbraEngine();

        public PenumbraComponent(Game game) : base(game)
        {
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

        public IList<Light> Lights => _engine.ObservableLights;
        public IList<Hull> Hulls => _engine.ObservableHulls;

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
