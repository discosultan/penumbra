using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics;
using Penumbra.Graphics.Providers;
using Penumbra.Graphics.Renderers;
using Penumbra.Utilities;

namespace Penumbra.Core
{
    internal class PenumbraEngine
    {
        private RasterizerState _rsCcw;
        private RasterizerState _rsCw;

        private ShadowRenderer _shadowRenderer;
        private LightRenderer _lightRenderer;

        private readonly LightmapTextureBuffer _textureBuffer = new LightmapTextureBuffer();
        private RenderProcessProvider _renderProcessProvider;
        private PrimitiveRenderer _primitivesRenderer;        

        private Color _ambientColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public PenumbraEngine(Projections projections)
        {
            Camera = new Camera(projections);
        }

        public bool Debug { get; set; } = true;

        public Color AmbientColor
        {
            get { return new Color(_ambientColor.R, _ambientColor.G, _ambientColor.B); }
            set { _ambientColor = new Color(value, 1f); }
        }

        public Matrix ViewProjection
        {
            get { return Camera.Custom; }
            set { Camera.Custom = value; }
        }

        internal ShaderParameterCollection ShaderParameters { get; } = new ShaderParameterCollection();
        internal ObservableCollection<Light> Lights { get; } = new ObservableCollection<Light>();
        internal ObservableCollection<Hull> Hulls { get; } = new ObservableCollection<Hull>();
        internal Camera Camera { get; }
        internal GraphicsDevice GraphicsDevice { get; private set; }
        internal RasterizerState RsDebug { get; private set;}

        internal RasterizerState Rs => Camera.InvertedY ? _rsCw : _rsCcw;        

        public void Load(GraphicsDevice device, GraphicsDeviceManager deviceManager, ContentManager content)
        {
            GraphicsDevice = device;
            BuildGraphicsResources();

            Camera.Load(GraphicsDevice, deviceManager);
            _textureBuffer.Load(GraphicsDevice, deviceManager);
            _renderProcessProvider = new RenderProcessProvider(GraphicsDevice, content, Camera);
            _primitivesRenderer = new PrimitiveRenderer(GraphicsDevice, this);            
            _shadowRenderer = new ShadowRenderer(GraphicsDevice, content, this);
            _lightRenderer = new LightRenderer(GraphicsDevice, content, this);

            // Setup logging for debug purposes.
            Logger.Add(new DelegateLogger(x => System.Diagnostics.Debug.WriteLine(x)));
        }

        public void PreRender()
        {
            GraphicsDevice.SetRenderTarget(_textureBuffer.Scene);
        }

        public void Render()
        {
            // Switch render target to lightmap.
            GraphicsDevice.SetRenderTarget(_textureBuffer.LightMap);
            GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target, AmbientColor, 1f, 0);

            ShaderParameters.SetMatrix(ShaderParameter.ViewProjection, ref Camera.ViewProjection);

            // Generate lightmap.
            int lightCount = Lights.Count;
            for (int i = 0; i < lightCount; i++)
            {
                Light light = Lights[i];
                if (!light.Enabled || !light.Intersects(Camera) || light.ContainedIn(Hulls))
                    continue;

                // Clear stencil.
                // TODO: use incremental stencil values to avoid clearing every light?
                if (light.ShadowType == ShadowType.Occluded)
                    GraphicsDevice.Clear(ClearOptions.Stencil, AmbientColor, 1f, 0);

                // Set scissor rectangle.                
                GraphicsDevice.SetScissorRectangle(Camera.GetScissorRectangle(light));

                // Draw shadows for light.
                if (light.CastsShadows)
                    _shadowRenderer.Render(light);

                // Draw light.                
                _lightRenderer.Render(light);

                // Clear light's dirty flags.
                light.DirtyFlags &= 0;
            }

            // Switch render target back to default.
            GraphicsDevice.SetRenderTarget(null);

            // Present lightmap.            
            _primitivesRenderer.DrawFullscreenQuad(_renderProcessProvider.Present, _textureBuffer.Scene);
            _primitivesRenderer.DrawFullscreenQuad(_renderProcessProvider.PresentLightmap, _textureBuffer.LightMap);

            // Clear hulls dirty flags.
            int hullCount = Hulls.Count;
            for (int i = 0; i < hullCount; i++)
                Hulls[i].DirtyFlags &= 0;
        }

        private void BuildGraphicsResources()
        {
            _rsCcw = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                ScissorTestEnable = true
            };
            _rsCw = new RasterizerState
            {
                CullMode = CullMode.CullClockwiseFace,
                ScissorTestEnable = true
            };
            RsDebug = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame,
                ScissorTestEnable = true
            };
        }
    }

    [Flags]
    public enum Projections
    {
        SpriteBatch = 1 << 0,
        OriginCenter_XRight_YUp = 1 << 1,
        OriginBottomLeft_XRight_YUp = 1 << 2,
        Custom = 1 << 3
    }
}
