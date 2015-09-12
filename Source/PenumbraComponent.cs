#if MONOGAME
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Penumbra
{
    public class PenumbraComponent : DrawableGameComponent
    {        
        private readonly PenumbraEngine _engine = new PenumbraEngine();

        public PenumbraComponent(Game game, Projections projections = Projections.SpriteBatch | Projections.Custom) 
            : base(game)
        {
            _engine.Camera.Projections = projections;

            Enabled = false;
            Visible = true;            
        }

        public bool Debug
        {
            get { return _engine.Debug; }
            set { _engine.Debug = value; }
        }

        public Color AmbientColor
        {
            get { return _engine.AmbientColor; }
            set { _engine.AmbientColor = value; }
        }

        public Matrix Transform
        {
            get { return _engine.Camera.Custom; }
            set { _engine.Camera.Custom = value; }
        }        

        public IList<Light> Lights => _engine.Lights;
        public IList<Hull> Hulls => _engine.Hulls;

        protected override void LoadContent()
        {            
            var graphicsDeviceManager = (GraphicsDeviceManager)Game.Services.GetService<IGraphicsDeviceManager>();
            _engine.Load(GraphicsDevice, graphicsDeviceManager, Game.Content);
        }

        public void BeginDraw() => _engine.PreRender();        

        public override void Draw(GameTime gameTime) => _engine.Render();        
    }
}
#endif