using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace Penumbra
{
    /// <summary>
    /// GPU based 2D lighting and shadowing engine with penumbra support. Operates with
    /// <see cref="Light"/>s and shadow <see cref="Hull"/>s, where light is a
    /// colored light source which casts shadows on shadow hulls that are outlines of scene
    /// geometry (polygons) impassable by light.
    /// </summary>
    /// <remarks>
    /// Before rendering scene, ensure to call <c>PenumbraComponent.BeginDraw</c> to swap render target
    /// in order for the component to be able to later apply generated lightmap.
    /// </remarks>
    public class PenumbraComponent : DrawableGameComponent
    {        
        private readonly PenumbraEngine _engine = new PenumbraEngine();        

        /// <summary>
        /// Constructs a new instance of <see cref="PenumbraComponent"/>.
        /// </summary>
        /// <param name="game">Game object to associate the engine with.</param>
        /// <param name="projections">
        /// Projections used to determined view projection matrix. These should be the same as used
        /// for the scene. See <see cref="Projections"/> for more info.
        /// </param>
        public PenumbraComponent(Game game, Projections projections = Projections.SpriteBatch | Projections.Custom) 
            : base(game)
        {
            _engine.Camera.Projections = projections;            

            Enabled = false;
            Visible = true;            
        }

        /// <summary>
        /// Gets or sets if debug outlines should be drawn for shadows and light sources.
        /// </summary>
        public bool Debug
        {
            get { return _engine.Debug; }
            set { _engine.Debug = value; }
        }
        /// <summary>
        /// Gets or sets the ambient color of the scene. Note that alpha value will be ignored.
        /// </summary>
        public Color AmbientColor
        {
            get { return _engine.AmbientColor; }
            set { _engine.AmbientColor = value; }
        }
        /// <summary>
        /// Gets or sets the custom transformation matrix used for view projection transform
        /// if <c>Projections.Custom</c> is set for the engine.
        /// </summary>
        public Matrix Transform
        {
            get { return _engine.Camera.Custom; }
            set { _engine.Camera.Custom = value; }
        }        
        /// <summary>
        /// Gets the list of lights registered with the engine.
        /// </summary>
        public ObservableCollection<Light> Lights => _engine.Lights;
        /// <summary>
        /// Gets the list of shadow hulls registered with the engine.
        /// </summary>
        public ObservableCollection<Hull> Hulls => _engine.Hulls;

        /// <summary>
        /// Explicitly loads the content for the engine. This should only be called if the
        /// component was not added to the game's components list through <c>Components.Add</c>.
        /// </summary>
        public void Load()
        {
            LoadContent();
        }       
        /// <summary>
        /// Sets up the lightmap generation sequence. This should be called before Draw.
        /// </summary>
        public void BeginDraw()
        {
            if (Visible)
                _engine.PreRender();
        }
        /// <summary>
        /// Generates the lightmap, blends it with whatever was drawn to the scene between the
        /// calls to BeginDraw and this and presents the result to the backbuffer.
        /// </summary>        
        /// <param name="gameTime">Time passed since the last call to Draw.</param>
        public override void Draw(GameTime gameTime)
        {
            if (Visible)
                _engine.Render();
        }

        protected override void LoadContent()
        {
            var deviceManager = (GraphicsDeviceManager)Game.Services.GetService<IGraphicsDeviceManager>();
            _engine.Load(GraphicsDevice, deviceManager, Game.Content);
        }
    }
}