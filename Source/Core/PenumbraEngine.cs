using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private DynamicShadowRenderer _dynamicShadowRenderer;

        private readonly LightmapTextureBuffer _textureBuffer = new LightmapTextureBuffer();
        private RenderProcessProvider _renderProcessProvider;
        private PrimitiveRenderer _primitivesRenderer;

        private Color _ambientColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public PenumbraEngine(Projections projections)
        {
            Camera = new Camera(projections);
            ResolvedHulls = new HullList(Hulls);
        }

        public bool DebugDraw { get; set; } = true;

        public Color AmbientColor
        {
            get { return new Color(_ambientColor.R, _ambientColor.G, _ambientColor.B); }
            set { _ambientColor = new Color(value, 1f); }
        }

        public Matrix ViewProjection
        {
            get { return Camera.CustomWorld; }
            set { Camera.CustomWorld = value; }
        }

        internal ShaderParameterCollection ShaderParameters { get; } = new ShaderParameterCollection();
        internal ObservableCollection<Light> Lights { get; } = new ObservableCollection<Light>();
        internal ObservableCollection<Hull> Hulls { get; } = new ObservableCollection<Hull>();
        internal Camera Camera { get; }
        internal GraphicsDevice GraphicsDevice { get; private set; }
        internal HullList ResolvedHulls { get; }

        public void Load(GraphicsDevice device, GraphicsDeviceManager deviceManager, ContentManager content)
        {
            GraphicsDevice = device;
            
            Camera.Load(GraphicsDevice, deviceManager);
            _textureBuffer.Load(GraphicsDevice, deviceManager);
            _renderProcessProvider = new RenderProcessProvider(GraphicsDevice, content, Camera);
            _primitivesRenderer = new PrimitiveRenderer(GraphicsDevice, this);            
            _dynamicShadowRenderer = new DynamicShadowRenderer(GraphicsDevice, content, this);

            // Setup logging for debug purposes.
            Logger.Add(new DelegateLogger(x => Debug.WriteLine(x)));
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

            ShaderParameters.SetMatrix(ShaderParameter.ProjectionTransform, ref Camera.WorldViewProjection);

            // Resolve hulls.
            ResolvedHulls.Resolve();

            // Generate lightmap.
            int lightCount = Lights.Count;
            for (int i = 0; i < lightCount; i++)
            {
                Light light = Lights[i];
                if (!light.Enabled) continue;
                if (ResolvedHulls.LightIsInside(light)) continue;

                // TODO: Cache and/or spatial tree?

                // Clear stencil.
                // TODO: use incremental stencil values to avoid clearing every light?
                if (light.ShadowType == ShadowType.Occluded)
                    GraphicsDevice.Clear(ClearOptions.Stencil, AmbientColor, 1f, 0);

                // Set scissor rectangle.                
                GraphicsDevice.ScissorRectangle = Camera.GetScissorRectangle(light);                

                // Draw shadows for light.
                if (light.CastsShadows)
                {
                    _dynamicShadowRenderer.DrawShadows(light);
                }

                // Draw light.                
                ShaderParameters.SetVector3(ShaderParameter.LightColor, light.Color.ToVector3());
                ShaderParameters.SetSingle(ShaderParameter.LightIntensity, light.IntensityFactor);
                _primitivesRenderer.DrawQuad(_renderProcessProvider.Light, light.Position, light.Range * 2);

                // Draw light source (for debugging purposes only).
                _primitivesRenderer.DrawCircle(_renderProcessProvider.LightSource, light.Position, light.Radius);

                // Clear alpha.                
                _primitivesRenderer.DrawFullscreenQuad(_renderProcessProvider.ClearAlpha);

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
            {                
                Hulls[i].DirtyFlags &= 0;
            }
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
