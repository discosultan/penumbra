using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics.Providers
{
    internal sealed class RenderProcessProvider : IDisposable
    {
        private static readonly object _lock = new object();
        private static int _refCounter;        
                        
        private static Lazy<RenderProcess> _umbraIlluminated;        
        private static Lazy<RenderProcess> _penumbraIlluminated;       
        private static Lazy<RenderProcess> _solidIlluminated;             
        private static Lazy<RenderProcess> _solidSolid;        
        private static Lazy<RenderProcess> _solidOccluded;        

        private static RenderProcess _light;
        private static RenderProcess _lightSource;
        private static RenderProcess _clearAlpha;
        private static RenderProcess _present;

        public RenderProcessProvider(GraphicsDevice graphicsDevice, ContentManager content)
        {
            lock (_lock)
            {
                if (_refCounter <= 0)
                {
                    Load(graphicsDevice, content);
                }
                _refCounter++;
            }
        }

        public RenderProcess Light => _light;
        public RenderProcess LightSource => _lightSource;
        public RenderProcess ClearAlpha => _clearAlpha;
        public RenderProcess Present => _present;

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
            lock (_lock)
            {
                _refCounter--;
                if (_refCounter <= 0)
                {
                    //Unload();
                }
            }
        }

        private static void Load(GraphicsDevice graphicsDevice, ContentManager content)
        {            
            var rsDebug = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame,
                ScissorTestEnable = true            
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
                CullMode = CullMode.CullCounterClockwiseFace,
                ScissorTestEnable = true
            };
            
            var bs2 = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.Zero,
                ColorWriteChannels = ColorWriteChannels.Alpha
            };

            _penumbraIlluminated = new Lazy<RenderProcess>(() => new RenderProcess(
                new RenderStep(new RenderState(dss, bs, rs), content.Load<Effect>("Penumbra"))
                , new RenderStep(new RenderState(DepthStencilState.None, BlendState.Opaque, rsDebug), content.Load<Effect>("Debug"), isDebug: true)
                ), isThreadSafe: true);
            _umbraIlluminated = new Lazy<RenderProcess>(() => new RenderProcess(
                new RenderStep(new RenderState(dss, bs2, rs), content.Load<Effect>("SolidDark")),
                //new RenderStep(new RenderState(dssLess, bs2, rs), ToDispose(effectSystem.LoadEffect("SolidDark"))),
                new RenderStep(new RenderState(DepthStencilState.None, BlendState.Opaque, rsDebug), content.Load<Effect>("Debug"), isDebug: true)
            ), isThreadSafe: true);
            _solidIlluminated = new Lazy<RenderProcess>(() => new RenderProcess(
                new RenderStep(new RenderState(dss, bs2, rs), content.Load<Effect>("SolidLit")),
                new RenderStep(new RenderState(DepthStencilState.None, BlendState.Opaque, rsDebug), content.Load<Effect>("Debug"), isDebug: true)
            ), isThreadSafe: true);           

            //**************//
            // SHADOW SOLID //
            //**************//        

            _solidSolid = new Lazy<RenderProcess>(() => new RenderProcess(
                new RenderStep(new RenderState(dss, bs2, rs), content.Load<Effect>("SolidDark")),
                new RenderStep(new RenderState(DepthStencilState.None, BlendState.Opaque, rsDebug), content.Load<Effect>("Debug"), isDebug: true)
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
                StencilFail = StencilOperation.Keep
            };

            _solidOccluded = new Lazy<RenderProcess>(() => new RenderProcess(
                new RenderStep(new RenderState(dss3, bs2, rs, 1), content.Load<Effect>("SolidLit"))
                , new RenderStep(new RenderState(DepthStencilState.None, BlendState.Opaque, rsDebug), content.Load<Effect>("Debug"), isDebug: true)
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
                //new RenderStep(new RenderState(dss, bs3, rs), ToDispose(effectSystem.LoadEffect("Light")))
                new RenderStep(new RenderState(dss, bs3, RasterizerState.CullCounterClockwise), content.Load<Effect>("Light"))                
                ,new RenderStep(new RenderState(DepthStencilState.None, BlendState.Opaque, rsDebug), content.Load<Effect>("DebugLight"), true)
            );
            _lightSource = new RenderProcess(
                new RenderStep(new RenderState(DepthStencilState.None, BlendState.Opaque, rsDebug), content.Load<Effect>("DebugLight"), true)
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
                new RenderStep(new RenderState(dss, bs4, rs), content.Load<Effect>("ClearAlpha"))
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
                new RenderStep(new RenderState(DepthStencilState.None, bs5, RasterizerState.CullCounterClockwise), 
                //new RenderStep(new RenderState(graphicsDevice.DepthStencilStates.None, graphicsDevice.BlendStates.Additive, graphicsDevice.RasterizerStates.CullNone), 
                    //effectSystem.LoadEffect("SpriteBase")) // TODO: How to keep this alive/dispose?
                    content.Load<Effect>("Present"))
            );
        }
    }
}
