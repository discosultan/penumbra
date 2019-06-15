// TODO: Features:
// TODO:    1.  Provide similar transforming capabilities for camera as are for light and hull. Currently
// TODO:        user must provide custom matrix for camera transformations.
// TODO:    2.  Occluded shadow type which illuminates the first hull the light ray intersects, but any hull
// TODO:        behind the ray is fully shadowed.
// TODO:    3.  Instead of specifying predefined shadow types for lights, use depth buffer instead to determine
// TODO:        illumination for hulls and allow users to change the height for hull or light. This would also
// TODO:        allow to render hulls in a single draw call instead of per light, since the illumination is no
// TODO:        longer dependant on the shadow type of a concrete light.
// TODO:    4.  Normal mapped lighting.

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

        public ObservableCollection<Light> Lights { get; } = new ObservableCollection<Light>();
        public HullList Hulls { get; } = new HullList();
        public CameraProvider Camera { get; } = new CameraProvider();
        public TextureProvider Textures { get; } = new TextureProvider();
        public ShadowRenderer ShadowRenderer { get; } = new ShadowRenderer();
        public LightRenderer LightRenderer { get; } = new LightRenderer();
        public LightMapRenderer LightMapRenderer { get; } = new LightMapRenderer();
        public GraphicsDevice Device { get; private set; }
        public GraphicsDeviceManager DeviceManager { get; private set; }
        public GameWindow Window { get; private set; }
        public RasterizerState RsDebug { get; private set;}
        private RasterizerState _rsCcw;
        private RasterizerState _rsCw;
        public RasterizerState Rs => Camera.InvertedY ? _rsCw : _rsCcw;

        public void Load(GraphicsDevice device, GraphicsDeviceManager deviceManager, GameWindow window,
            Effect fxHull, Effect fxLight, Effect fxShadow, Effect fxTexture)
        {
            Device = device;
            DeviceManager = deviceManager;
            Window = window;

            BuildGraphicsResources();

            // Load providers.
            Camera.Load(this);
            Textures.Load(this);

            // Load renderers.
            LightMapRenderer.Load(this, fxTexture);
            ShadowRenderer.Load(this, fxShadow, fxHull);
            LightRenderer.Load(this, fxLight);
        }

        public void PreRender()
        {
            // Store currently active render targets so we can reset them once we are done blending the lightmap.
            Device.GetRenderTargets(Textures.GetOriginalRenderTargetBindingsForQuery());

            // Switch render target to a diffuse map. This is where users will render content to be lit.
            Device.SetRenderTargets(Textures.DiffuseMapBindings);
        }

        public void Render()
        {
            // Update hulls internal data structures.
            Hulls.Update();

            // We want to use clamping sampler state throughout the lightmap rendering process.
            // This is required when drawing lights. Since light rendering and alpha clearing is done
            // in a single step, light is rendered with slightly larger quad where tex coords run out of the [0..1] range.
            Device.SamplerStates[0] = SamplerState.LinearClamp;

            // Switch render target to lightmap.
            Device.SetRenderTargets(Textures.LightmapBindings);

            // Clear lightmap color, depth and stencil data.
            Device.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target, _ambientColor, 1f, 0);

            // Set per frame shader data.
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
                Camera.GetScissorRectangle(light, out BoundingRectangle scissor);
                Device.SetScissorRectangle(ref scissor);

                // Mask shadowed areas by reducing alpha.
                ShadowRenderer.Render(light);

                // Draw light and clear alpha (reset it to 1 [fully lit] for next light).
                LightRenderer.Render(light);

                // Clear light's dirty flag.
                light.Dirty = false;
            }

            // Switch render target back to default.
            Device.SetRenderTargets(Textures.GetOriginalRenderTargetBindings());

            // Blend original scene and lightmap and present to backbuffer.
            LightMapRenderer.Present();

            // Clear hulls dirty flag.
            Hulls.Dirty = false;
        }

        public void Dispose()
        {
            RsDebug?.Dispose();
            _rsCw?.Dispose();
            _rsCcw?.Dispose();
            LightRenderer.Dispose();
            LightMapRenderer.Dispose();
            ShadowRenderer.Dispose();
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
}