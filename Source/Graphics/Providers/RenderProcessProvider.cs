using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics.Providers
{
    internal sealed class RenderProcessProvider : IDisposable
    {
        private readonly ContentManager _content;
        private readonly GraphicsDevice _graphics;
        private readonly Camera _camera;

        private Lazy<RenderProcess> _umbraIlluminated;        
        private Lazy<RenderProcess> _penumbraIlluminated;
        private Lazy<RenderProcess> _antumbraIlluminated;
        private Lazy<RenderProcess> _solidIlluminated;             
        private Lazy<RenderProcess> _solidSolid;        
        private Lazy<RenderProcess> _solidOccluded;        

        private RenderProcess _light;
        private RenderProcess _lightSource;
        private RenderProcess _clearAlpha;
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

        public RenderProcess Light => _light;
        public RenderProcess LightSource => _lightSource;
        public RenderProcess ClearAlpha => _clearAlpha;
        public RenderProcess Present => _present;
        public RenderProcess PresentLightmap => _presentLightmap;

        public RenderProcess Umbra(ShadowType shadowType)
        {            
            switch (shadowType)
            {
                case ShadowType.Solid:                    
                case ShadowType.Occluded:                    
                default: // umbra illuminated
                    return _umbraIlluminated.Value;
            }
        }

        public RenderProcess Penumbra(ShadowType shadowType)
        {
            switch (shadowType)
            {
                case ShadowType.Solid:                    
                case ShadowType.Occluded:                    
                default: // illuminated
                    return _penumbraIlluminated.Value;
            }
        }

        public RenderProcess Antumbra(ShadowType shadowType)
        {
            switch (shadowType)
            {
                case ShadowType.Solid:
                case ShadowType.Occluded:
                default: // illuminated
                    return _antumbraIlluminated.Value;
            }
        }

        public RenderProcess Solid(ShadowType shadowType)
        {
            switch (shadowType)
            {
                case ShadowType.Solid:
                    return _solidSolid.Value; 
                case ShadowType.Occluded:
                     return _solidOccluded.Value;                    
                default: // illuminated
                     return _solidIlluminated.Value;
            }
        }

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

            var rsDebug = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame,
                ScissorTestEnable = false
                //ScissorTestEnable = true
            };

            //********************//
            // SHADOW ILLUMINATED //
            //********************//

            var bs = new BlendState
            {                
                AlphaBlendFunction = BlendFunction.Subtract,
                AlphaSourceBlend = Blend.DestinationAlpha,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                ColorWriteChannels = ColorWriteChannels.Alpha
            };                  
            
            var dss = new DepthStencilState
            {
                StencilEnable = false,
                DepthBufferEnable = false
            };
            
            var rs = new RasterizerState
            {
                CullMode = GetCCWCullMode(),
                //ScissorTestEnable = true                
                ScissorTestEnable = false
            };
            
            var bs2 = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.Zero,
                ColorWriteChannels = ColorWriteChannels.Alpha
            };

            _penumbraIlluminated = new Lazy<RenderProcess>(() => new RenderProcess(
                NewRenderStep(device, content, dss, bs, rs, "ProjectionPenumbra"),
                NewRenderStep(device, content, DepthStencilState.None, BlendState.Opaque, rsDebug, "ProjectionColor", true, x => x.SetVector4(ShaderParameter.Color, Color.Red.ToVector4()))                
                ), isThreadSafe: true);
            _antumbraIlluminated = new Lazy<RenderProcess>(() => new RenderProcess(
                NewRenderStep(device, content, dss, bs, rs, "ProjectionPenumbra"),
                NewRenderStep(device, content, DepthStencilState.None, BlendState.Opaque, rsDebug, "ProjectionColor", true, x => x.SetVector4(ShaderParameter.Color, Color.Orange.ToVector4()))
                ), isThreadSafe: true);
            _umbraIlluminated = new Lazy<RenderProcess>(() => new RenderProcess(
                NewRenderStep(device, content, dss, bs2, rs, "ProjectionColor", addParams: x => x.SetVector4(ShaderParameter.Color, Color.Transparent.ToVector4())),
                NewRenderStep(device, content, DepthStencilState.None, BlendState.Opaque, rsDebug, "ProjectionColor", true, x => x.SetVector4(ShaderParameter.Color, Color.Green.ToVector4()))
            ), isThreadSafe: true);
            _solidIlluminated = new Lazy<RenderProcess>(() => new RenderProcess(
                NewRenderStep(device, content, dss, bs2, rs, "ProjectionColor", addParams: x => x.SetVector4(ShaderParameter.Color, Color.White.ToVector4())),
                NewRenderStep(device, content, DepthStencilState.None, BlendState.Opaque, rsDebug, "ProjectionColor", true, x => x.SetVector4(ShaderParameter.Color, Color.Blue.ToVector4()))
            ), isThreadSafe: true);           

            //**************//
            // SHADOW SOLID //
            //**************//        

            _solidSolid = new Lazy<RenderProcess>(() => new RenderProcess(
                NewRenderStep(device, content, dss, bs2, rs, "ProjectionColor", addParams: x => x.SetVector4(ShaderParameter.Color, Color.Transparent.ToVector4())),
                NewRenderStep(device, content, DepthStencilState.None, BlendState.Opaque, rsDebug, "ProjectionColor", true, x => x.SetVector4(ShaderParameter.Color, Color.Blue.ToVector4()))
            ), isThreadSafe: true);

            //*****************//
            // SHADOW OCCLUDED //
            //*****************//

            var dss2 = new DepthStencilState
            {
                DepthBufferEnable = false,
                StencilEnable = true,
                StencilWriteMask = 0xff,
                StencilMask = 0x00,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.IncrementSaturation,
                StencilFail = StencilOperation.Keep
            };

            var dss3 = new DepthStencilState
            {
                DepthBufferEnable = false,
                StencilEnable = true,
                StencilWriteMask = 0x00,
                StencilMask = 0xff,
                StencilFunction = CompareFunction.Equal,
                StencilPass = StencilOperation.Keep,
                StencilFail = StencilOperation.Keep,
                ReferenceStencil = 1
            };

            _solidOccluded = new Lazy<RenderProcess>(() => new RenderProcess(
                NewRenderStep(device, content, dss3, bs2, rs, "ProjectionColor", addParams: x => x.SetVector4(ShaderParameter.Color, Color.White.ToVector4()))
                , NewRenderStep(device, content, DepthStencilState.None, BlendState.Opaque, rsDebug, "ProjectionColor", true, x => x.SetVector4(ShaderParameter.Color, Color.Blue.ToVector4()))
            ), isThreadSafe: true);

            //*******//
            // LIGHT //
            //*******//
                        
            var bs3 = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationAlpha,
                ColorDestinationBlend = Blend.One,
                ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue
            };

            _light = new RenderProcess(                
                NewRenderStep(device, content, dss, bs3, GetCCWRasterizerState(), "WorldProjectionLight")                
                //, NewRenderStep(device, content, DepthStencilState.None, BlendState.Opaque, rsDebug, "WorldProjectionColor", true, x => x.SetVector4(ShaderParameter.Color, Color.Yellow.ToVector4()))
            );
            _lightSource = new RenderProcess(
                NewRenderStep(device, content, DepthStencilState.None, BlendState.Opaque, rsDebug, "WorldProjectionColor", true, x => x.SetVector4(ShaderParameter.Color, Color.Purple.ToVector4()))
            );

            //*************//
            // CLEAR ALPHA //
            //*************//
            
            var bs4 = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                ColorWriteChannels = ColorWriteChannels.Alpha
            };

            _clearAlpha = new RenderProcess(
                NewRenderStep(device, content, dss, bs4, rs, "Color", addParams: x => x.SetVector4(ShaderParameter.Color, Color.White.ToVector4()))
            );

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
