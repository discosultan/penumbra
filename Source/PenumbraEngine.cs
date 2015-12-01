// TODO: Features:
// TODO:    1.  Occluded shadow type to work correctly for concave hulls.
// TODO:    2.  Instead of specifying predefined shadow types for lights, use depth buffer instead to determine
// TODO:        illumination for hulls and allow users to change the height for hull or light. This would also
// TODO:        allow to render hulls in a single draw call instead of per light, since the illumination is no
// TODO:        longer dependant on the shadow type of a concrete light.

using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Geometry;
using Penumbra.Graphics;
using Penumbra.Graphics.Providers;
using Penumbra.Graphics.Renderers;
using Penumbra.Utilities;

namespace Penumbra
{
    internal class PenumbraEngine : IDisposable
    {        
        private readonly ILogger _delegateLogger = new DelegateLogger(x => System.Diagnostics.Debug.WriteLine(x));

        private readonly DebugRenderer RenderHelper = new DebugRenderer();
        private readonly ShadowRenderer ShadowRenderer = new ShadowRenderer();
        private readonly LightRenderer LightRenderer = new LightRenderer();
        private readonly LightmapRenderer LightMapRenderer = new LightmapRenderer();

        private bool _renderTargetStored;

        private Color _nonPremultipliedAmbient = Color.DarkSlateGray;
        private Vector4 _ambientColor = Color.DarkSlateGray.ToVector4();
        public Color AmbientColor
        {
            get { return _nonPremultipliedAmbient; }
            set
            {                
                _nonPremultipliedAmbient = value;
                Calculate.FromNonPremultiplied(value, out _ambientColor);
                _ambientColor.W = 1.0f;                
            }
        }

        private bool _debug;
        public bool Debug
        {
            get { return _debug; }
            set
            {
                if (_debug != value)                
                {
                    if (value)
                        Logger.Add(_delegateLogger);                    
                    else
                        Logger.Remove(_delegateLogger);
                    _debug = value;
                }                
            }
        }

        public bool NormalMappedLightingEnabled { get; set; }
        public ObservableCollection<Light> Lights { get; } = new ObservableCollection<Light>();
        public HullList Hulls { get; } = new HullList();
        public CameraProvider Camera { get; } = new CameraProvider();
        public TextureProvider Textures { get; } = new TextureProvider();        
        public GraphicsDevice GraphicsDevice { get; private set; }
        public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }        

        private RasterizerState _rasterizerStateCcw;
        private RasterizerState _rasterizerStateCw;
        public RasterizerState RasterizerState => Camera.InvertedY ? _rasterizerStateCw : _rasterizerStateCcw;
        public RasterizerState RasterizerStateDebug { get; private set; }

        public void Initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager)
        {
            GraphicsDevice = graphicsDevice;
            GraphicsDeviceManager = graphicsDeviceManager;

            BuildGraphicsResources();

            // Initialize graphics providers.
            Camera.Initialize(this);
            Textures.Initialize(this);

            // Initialize renderers.
            LightMapRenderer.Initialize(this);
            ShadowRenderer.Initialize(this);
            LightRenderer.Initialize(this);
            RenderHelper.Initialize(this);
        }                

        public void BeginNormalMap()
        {
            StoreRenderTargetIfRequired();   

            // Switch render target to a normal map. Users will render scene normals on it.
            GraphicsDevice.SetRenderTargets(Textures.NormalMapBindings);            
        }

        public void BeginDiffuseMap()
        {
            StoreRenderTargetIfRequired();

            // Switch render target to a diffuse map. Users will render scene content to be lit on it.
            GraphicsDevice.SetRenderTargets(Textures.DiffuseMapBindings);            
        }
        
        public void Render()
        {
            // Update hulls internal data structures.
            Hulls.Update();
                     
            // We want to use clamping sampler state throughout the lightmap rendering process.
            // This is required when drawing lights. Since light rendering and alpha clearing is done 
            // in a single step, light is rendered with slightly larger quad where tex coords run out of the [0..1] range.
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            // Switch render target to lightmap.
            GraphicsDevice.SetRenderTargets(Textures.LightmapBindings);

            // Clear lightmap color, depth and stencil data.
            GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target, _ambientColor, 1f, 0);

            // Set per frame shader data.
            LightRenderer.PreRender();
            ShadowRenderer.PreRender();

            // Generate lightmap. For each light, mask the shadowed areas determined by hulls and render light.
            int lightCount = Lights.Count;
            for (int i = 0; i < lightCount; i++)
            {
                Light light = Lights[i];

                // Continue only if light is enabled and not inside any hull.
                if (!light.Enabled || Hulls.Contains(light))
                    continue;

                // Update light's internal data structures.
                light.Update();

                // Continue only if light is within camera view.
                if (!light.Intersects(Camera))                
                    continue;

                // Set scissor rectangle to clip any shadows outside of light's range.
                BoundingRectangle scissor;
                Camera.GetScissorRectangle(light, out scissor);
                GraphicsDevice.SetScissorRectangle(ref scissor);

                // Mask shadowed areas by reducing alpha.                
                ShadowRenderer.Render(light);

                // Draw light and clear alpha (reset it to 1 [fully lit] for next light).
                LightRenderer.Render(light);

                // Clear light's dirty flag.
                light.Dirty = false;
            }

            // Switch render target back to default.
            GraphicsDevice.SetRenderTargets(Textures.GetOriginalRenderTargetBindings());

            // Blend original scene and lightmap and present to backbuffer.
            LightMapRenderer.Render();

            if (Debug) // TODO: Proper handling
            {
                const int width = 300;
                float aspect = (float) Textures.NormalMap.Height/Textures.NormalMap.Width;
                RenderHelper.Render(Textures.NormalMap, new Rectangle(0, 0, width, (int) (width*aspect)));
            }

            // Reset per frame flags.
            Hulls.Dirty = false;
            _renderTargetStored = false;
        }

        public void Dispose()
        {
            RasterizerStateDebug?.Dispose();
            _rasterizerStateCw?.Dispose();
            _rasterizerStateCcw?.Dispose();
            LightRenderer.Dispose();
            LightMapRenderer.Dispose();
            ShadowRenderer.Dispose();
        }

        private void BuildGraphicsResources()
        {
            _rasterizerStateCcw = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                ScissorTestEnable = true
            };
            _rasterizerStateCw = new RasterizerState
            {
                CullMode = CullMode.CullClockwiseFace,
                ScissorTestEnable = true
            };
            RasterizerStateDebug = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame,
                ScissorTestEnable = true
            };
        }

        private void StoreRenderTargetIfRequired()
        {
            if (!_renderTargetStored)
            {
                // Store currently active render targets so we can reset them once we are done blending the lightmap.
                GraphicsDevice.GetRenderTargets(Textures.GetOriginalRenderTargetBindingsForQuery());
                _renderTargetStored = true;
            }
        }
    }
}