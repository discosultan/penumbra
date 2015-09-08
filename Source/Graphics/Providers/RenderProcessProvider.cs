using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Core;

namespace Penumbra.Graphics.Providers
{
    internal sealed class RenderProcessProvider : IDisposable
    {
        private readonly ContentManager _content;
        private readonly GraphicsDevice _graphics;
        private readonly Camera _camera;

        private RenderProcess _light;
        private RenderProcess _lightSource;
        private RenderProcess _present;
        private RenderProcess _presentLightmap;        

        public RenderProcessProvider(GraphicsDevice graphicsDevice, ContentManager content, Camera camera)
        {
            _graphics = graphicsDevice;
            _content = content;
            _camera = camera;
            _camera.YInverted += (s, e) => 
            {
                Dispose();
                Load();
            };
            Load();
        }
        public RenderProcess Present => _present;
        public RenderProcess PresentLightmap => _presentLightmap;


        public void Dispose()
        {
        }

        private static RenderStep NewRenderStep(GraphicsDevice device, ContentManager content, DepthStencilState dss, BlendState bs,
            RasterizerState rs, string effectName, bool debug = false, Action<ShaderParameterCollection> addParams = null)
        {
            var parameters = new List<ShaderParameter>();
            var effect = content.Load<Effect>(effectName);
            foreach (var param in effect.Parameters)
            {
                parameters.Add((ShaderParameter)Enum.Parse(typeof (ShaderParameter), param.Name));
            }

            return new RenderStep(
                device,
                new RenderState(dss, bs, rs),
                effect,
                parameters.ToArray(),
                debug,
                addParams);
        }

        private void Load()
        {
            GraphicsDevice device = _graphics;
            ContentManager content = _content;

            //*********//
            // PRESENT //
            //*********//

            var bs5 = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.SourceColor,
                ColorSourceBlend = Blend.Zero,
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.SourceAlpha,
                AlphaSourceBlend = Blend.Zero,
                ColorWriteChannels = ColorWriteChannels.All
            };

            _present = new RenderProcess(                
                NewRenderStep(device, content, DepthStencilState.None, BlendState.Opaque, RasterizerState.CullCounterClockwise, "Texture")
            );

            _presentLightmap = new RenderProcess(
                NewRenderStep(device, content, DepthStencilState.None, bs5, RasterizerState.CullCounterClockwise, "Texture")
            );
        }

        private CullMode GetCCWCullMode()
        {
            return _camera.InvertedY ? CullMode.CullClockwiseFace : CullMode.CullCounterClockwiseFace;
        }

        private RasterizerState GetCCWRasterizerState()
        {
            return _camera.InvertedY ? RasterizerState.CullClockwise : RasterizerState.CullCounterClockwise;
        }
    }
}
