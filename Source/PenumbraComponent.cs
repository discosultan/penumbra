using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics;
using Penumbra.Graphics.Helpers;
using Penumbra.Graphics.Providers;
using Penumbra.Utilities;

namespace Penumbra
{
    public class PenumbraComponent : DrawableGameComponent
    {        
        private BufferedShadowRenderHelper _bufferedCPUShadowRenderHelper;

        private readonly Camera _camera = new Camera();
        private readonly LightmapTextureBuffer _textureBuffer = new LightmapTextureBuffer();        
        private RenderProcessProvider _renderProcessProvider;
        private PrimitiveRenderHelper _primitiveRenderHelper;

        private Color _ambientColor = Color.White;

        public PenumbraComponent(Game game) : base(game)
        {
            Enabled = false;
            Visible = true;

            // Setup logging for debug purposes.
            Logger.Add(new DelegateLogger(x => Debug.WriteLine(x)));

            // Set default values.            
            AmbientColor = new Color(0.2f, 0.2f, 0.2f);
            DebugDraw = true;
            ViewProjection = Matrix.Identity;
        }

        public IList<Light> Lights => ObservableLights;
        public IList<Hull> Hulls => ObservableHulls;

        public bool DebugDraw { get; set; }

        public Color AmbientColor
        {
            get { return new Color(_ambientColor.R, _ambientColor.G, _ambientColor.B); }
            set { _ambientColor = new Color(value, 1f); }
        }

        public Matrix ViewProjection
        {
            get { return _camera.ViewProjection; }
            set { _camera.ViewProjection = value; }
        }

        internal ShaderParameterCollection ShaderParameters { get; } = new ShaderParameterCollection();
        internal ObservableCollection<Light> ObservableLights { get; } = new ObservableCollection<Light>();
        internal ObservableCollection<Hull> ObservableHulls { get; } = new ObservableCollection<Hull>();
        internal Camera Camera => _camera;

        protected override void LoadContent()
        {
            base.LoadContent();
            GraphicsDeviceManager graphicsDeviceManager = (GraphicsDeviceManager)Game.Services.GetService<IGraphicsDeviceManager>();
            _camera.Load(GraphicsDevice, graphicsDeviceManager);
            _textureBuffer.Load(GraphicsDevice, graphicsDeviceManager);
            _renderProcessProvider = new RenderProcessProvider(GraphicsDevice, Game.Content);
            _primitiveRenderHelper = new PrimitiveRenderHelper(GraphicsDevice, this);            
            _bufferedCPUShadowRenderHelper = new BufferedShadowRenderHelper(GraphicsDevice, this);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            // Switch render target to lightmap.
            GraphicsDevice.SetRenderTarget(_textureBuffer.LightMap);
            GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target, AmbientColor, 0f, 0);
            
            ShaderParameters.SetMatrix(ShaderParameter.ProjectionTransform, ref _camera.ViewProjection);

            // Generate lightmap.
            foreach (Light light in Lights)
            {
                if (!light.Enabled) continue;

                // TODO: Cache and/or spatial tree?
                if (Hulls.Any(hull => light.IsInside(hull))) continue;

                // Clear stencil.
                // TODO: use incremental stencil values to avoid clearing every light?
                if (light.ShadowType == ShadowType.Occluded)
                    GraphicsDevice.Clear(ClearOptions.Stencil, AmbientColor, 0f, 0);

                // Set scissor rectangle.
                // DO NOT USE params overload. Causes unnecessary garbage.                
                GraphicsDevice.ScissorRectangle = _camera.GetScissorRectangle(light);

                ShaderParameters.SetVector3(ShaderParameter.LightColor, light.Color.ToVector3());
                ShaderParameters.SetSingle(ShaderParameter.LightRadius, light.Radius);
                ShaderParameters.SetSingle(ShaderParameter.LightRange, light.Range);
                ShaderParameters.SetVector2(ShaderParameter.LightPosition, light.Position);
                ShaderParameters.SetSingle(ShaderParameter.LightIntensity, light.Intensity);                

                // Draw shadows for light.
                if (light.CastsShadows)
                {
                    _bufferedCPUShadowRenderHelper.DrawShadows(
                        light,                        
                        _renderProcessProvider.Umbra(light.ShadowType),
                        _renderProcessProvider.Penumbra(light.ShadowType),
                        _renderProcessProvider.Solid(light.ShadowType));
                }

                // Draw light.                
                _primitiveRenderHelper.DrawQuad(_renderProcessProvider.Light, light.Position, light.Range * 2, Color.Yellow);

                // Draw light source (for debugging purposes only).
                _primitiveRenderHelper.DrawCircle(_renderProcessProvider.LightSource, light.Position, light.Radius, Color.Purple);

                // Clear alpha.                
                _primitiveRenderHelper.DrawFullscreenQuad(_renderProcessProvider.ClearAlpha);

                // Clear light's dirty flags.
                light.DirtyFlags &= 0;
            }

            // Switch render target back to default.
            GraphicsDevice.SetRenderTarget(null);

            ShaderParameters.SetTexture(ShaderParameter.Texture, _textureBuffer.LightMap);
            ShaderParameters.SetSampler(ShaderParameter.TextureSampler, SamplerState.LinearClamp);            

            // Present lightmap.            
            _primitiveRenderHelper.DrawFullscreenQuad(_renderProcessProvider.Present);

            // Clear hulls dirty flags.
            foreach (Hull hull in Hulls)
            {
                hull.DirtyFlags &= 0;
            }
        }
    }

}
