// TODO: Performance improvements:
// TODO:    1.  Update constant buffers based on their usage frequency (ie per frame / per object etc).
// TODO: Features:
// TODO:    1.  Provide similar transforming capabilities for camera as are for light and hull. Currently
// TODO:        user must provide custom matrix for camera transformations.
// TODO:    2.  Occluded shadow type which illuminates the first hull the light ray intersects, but any hull
// TODO:        behind the ray is fully shadowed.
// TODO:    3.  Instead of specifying predefined shadow types for lights, use depth buffer instead to determine
// TODO:        illumination for hulls and allow users to change the height for hull or light. This would also
// TODO:        allow to render hulls in a single draw call instead of per light, since the illumination is no
// TODO:        longer dependant on the shadow type of a concrete light.

using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics;
using Penumbra.Graphics.Providers;
using Penumbra.Graphics.Renderers;
using Penumbra.Utilities;

namespace Penumbra
{
    internal class PenumbraEngine
    {
        private RasterizerState _rsCcw;
        private RasterizerState _rsCw;

        private Color _ambientColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public Color AmbientColor
        {
            get { return new Color(_ambientColor.R, _ambientColor.G, _ambientColor.B); }
            set { _ambientColor = new Color(value, 1f); }
        }
        public bool Debug { get; set; }
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
            // Switch render target to custom scene texture.
            Device.SetRenderTarget(Textures.Scene);
        }
        
        public void Render()
        {            
            // We want to use clamping sampler state throughout the lightmap rendering process.
            // This is required when drawing lights. Since light rendering and alpha clearing is done 
            // in a single step, light is rendered with slightly larger quad where tex coords run out of the [0..1] range.
            Device.SamplerStates[0] = SamplerState.LinearClamp;

            // Switch render target to lightmap.
            Device.SetRenderTarget(Textures.LightMap);

            // Clear lightmap color, depth and stencil data.
            Device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target, _ambientColor, 1f, 0);

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
                ShadowRenderer.Render(light);

                // Draw light and clear alpha.
                LightRenderer.Render(light);

                // Clear light's dirty flags.
                light.DirtyFlags &= 0;
            }

            // Switch render target back to default.
            Device.SetRenderTarget(null);

            // Blend original scene and lightmap and present to backbuffer.
            LightMapRenderer.Present();

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