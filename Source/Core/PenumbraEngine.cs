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

        public ObservableCollection<Light> Lights { get; } = new ObservableCollection<Light>();
        public ObservableCollection<Hull> Hulls { get; } = new ObservableCollection<Hull>();
        public CameraProvider Camera { get; } = new CameraProvider();
        public TextureProvider Textures { get; } = new TextureProvider();
        public ShadowRenderer ShadowRenderer { get; } = new ShadowRenderer();
        public LightRenderer LightRenderer { get; } = new LightRenderer();
        public LightMapRenderer LightMapRenderer { get; } = new LightMapRenderer();
        public GraphicsDevice Device { get; private set; }
        public GraphicsDeviceManager DeviceManager { get; private set; }
        public ContentManager Content { get; private set; }
        public RasterizerState RsDebug { get; private set;}
        public RasterizerState Rs => Camera.InvertedY ? _rsCw : _rsCcw;
        public bool Debug { get; set; } = true;

        public void Load(GraphicsDevice device, GraphicsDeviceManager deviceManager, ContentManager content)
        {
            Device = device;
            DeviceManager = deviceManager;
            Content = content;

            BuildGraphicsResources();

            // Load providers.
            Camera.Load(this);
            Textures.Load(this);

            // Load renderers.
            LightMapRenderer.Load(this);
            ShadowRenderer.Load(this);
            LightRenderer.Load(this);

            // Setup logging for debug purposes.
            Logger.Add(new DelegateLogger(x => System.Diagnostics.Debug.WriteLine(x)));
        }

        public void PreRender()
        {
            LightMapRenderer.PreRenderSetup();            
        }

        public void Render()
        {
            LightMapRenderer.RenderSetup();

            // Generate lightmap.
            int lightCount = Lights.Count;
            for (int i = 0; i < lightCount; i++)
            {
                Light light = Lights[i];
                if (!light.Enabled || !light.Intersects(Camera) || light.ContainedIn(Hulls))
                    continue;

                // Set scissor rectangle.                
                Device.SetScissorRectangle(Camera.GetScissorRectangle(light));

                // Draw shadows for light.
                if (light.CastsShadows)
                    ShadowRenderer.Render(light);

                // Draw light.                
                LightRenderer.Render(light);

                // Clear light's dirty flags.
                light.DirtyFlags &= 0;
            }

            LightMapRenderer.Render();

            // Clear hulls dirty flags.
            Hulls.ClearDirtyFlags();
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