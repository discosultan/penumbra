using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;
using Polygon = Penumbra.Utilities.FastList<Microsoft.Xna.Framework.Vector2>;

namespace Penumbra.Graphics.Renderers
{
    internal class ShadowRenderer
    {
        private static readonly Color DebugColor = Color.Red;

        private readonly FastList<VertexShadow> _shadowVertices = new FastList<VertexShadow>();
        private readonly FastList<Vector2> _hullVertices = new FastList<Vector2>();
        private readonly FastList<int> _shadowIndices = new FastList<int>();
        private readonly FastList<int> _hullIndices = new FastList<int>();
        private readonly Dictionary<Light, Tuple<DynamicVao, DynamicVao>> _lightsVaos =
            new Dictionary<Light, Tuple<DynamicVao, DynamicVao>>();

        private PenumbraEngine _engine;

        private Effect _fxShadow;
        private EffectTechnique _fxShadowTech;
        private EffectTechnique _fxShadowTechDebug;
        private EffectParameter _fxShadowParamLightPosition;
        private EffectParameter _fxShadowParamLightRadius;
        private EffectParameter _fxShadowParamVp;
        private Effect _fxHull;
        private EffectTechnique _fxHullTech;
        private EffectParameter _fxHullParamVp;
        private EffectParameter _fxHullParamColor;
        private BlendState _bsShadow;
        private BlendState _bsHull;
        //private DepthStencilState _dsOccludedShadow;
        //private DepthStencilState _dsOccludedHull;

        public void Load(PenumbraEngine engine)
        {            
            _engine = engine;

            _fxShadow = engine.Content.Load<Effect>("Shadow");
            _fxShadowTech = _fxShadow.Techniques["Main"];
            _fxShadowTechDebug = _fxShadow.Techniques["Debug"];
            _fxShadowParamLightPosition = _fxShadow.Parameters["LightPosition"];
            _fxShadowParamLightRadius = _fxShadow.Parameters["LightRadius"];
            _fxShadowParamVp = _fxShadow.Parameters["ViewProjection"];

            _fxShadow.Parameters["Color"].SetValue(DebugColor.ToVector4());

            _fxHull = engine.Content.Load<Effect>("Hull");
            _fxHullTech = _fxHull.Techniques["Main"];
            _fxHullParamVp = _fxHull.Parameters["ViewProjection"];
            _fxHullParamColor = _fxHull.Parameters["Color"];
            
            BuildGraphicsResources();
        }

        public void PreRender()
        {            
            _fxShadowParamVp.SetValue(_engine.Camera.ViewProjection);
        }

        public void Render(Light light)
        {
            Tuple<DynamicVao, DynamicVao> vao = TryGetVaoForLight(light);
            if (vao == null)
                return;

            _engine.Device.RasterizerState = _engine.Rs;
            _engine.Device.DepthStencilState = DepthStencilState.None;
            //_engine.Device.DepthStencilState = light.ShadowType == ShadowType.Occluded 
            //    ? _dsOccludedShadow 
            //    : DepthStencilState.None;            

            if (light.CastsShadows)
            {
                _fxShadowParamLightPosition.SetValue(light.Position);
                _fxShadowParamLightRadius.SetValue(light.Radius);

                // Draw shadows.
                var shadowVao = vao.Item1;
                _engine.Device.BlendState = _bsShadow;                                                
                _engine.Device.SetVertexArrayObject(shadowVao);
                _fxShadowTech.Passes[0].Apply();
                _engine.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shadowVao.VertexCount, 0, shadowVao.IndexCount / 3);                

                // Draw shadows borders if debugging.
                if (_engine.Debug)
                {
                    _engine.Device.RasterizerState = _engine.RsDebug;
                    _engine.Device.BlendState = BlendState.Opaque;                                       
                    _fxShadowTechDebug.Passes[0].Apply();                    
                    _engine.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shadowVao.VertexCount, 0, shadowVao.IndexCount / 3);                    
                }
            }

            // Draw hulls.            
            //if (light.ShadowType == ShadowType.Occluded)
            //    _engine.Device.DepthStencilState = _dsOccludedHull;

            if (light.CastsShadows || light.ShadowType == ShadowType.Solid)
            {
                var hullVao = vao.Item2;
                _engine.Device.RasterizerState = _engine.Rs;
                _engine.Device.BlendState = _bsHull;
                _fxHullParamVp.SetValue(_engine.Camera.ViewProjection);
                _fxHullParamColor.SetValue(light.ShadowType == ShadowType.Solid
                    ? Color.Transparent.ToVector4()
                    : Color.White.ToVector4());
                _engine.Device.SetVertexArrayObject(hullVao);
                _fxHullTech.Passes[0].Apply();
                _engine.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, hullVao.VertexCount, 0, hullVao.IndexCount / 3);
            }
        }

        private Tuple<DynamicVao, DynamicVao> TryGetVaoForLight(Light light)
        {
            if (light.Dirty || _engine.Hulls.Dirty)
                return TryBuildVaoForLight(light);

            Tuple<DynamicVao, DynamicVao> vao;
            _lightsVaos.TryGetValue(light, out vao);
            return vao;
        }

        private Tuple<DynamicVao, DynamicVao> TryBuildVaoForLight(Light light)
        {                        
            _hullVertices.Clear();
            _shadowVertices.Clear();
            _shadowIndices.Clear();
            _hullIndices.Clear();

            int numSegments = 0;
            int shadowIndexOffset = 0;
            int hullIndexOffset = 0;
            int hullCount = _engine.Hulls.Count;
            for (int i = 0; i < hullCount; i++)
            {
                Hull hull = _engine.Hulls[i];
                if (!hull.Enabled || !hull.Valid || !light.Intersects(hull))
                    continue;
                
                Polygon points = hull.WorldPoints;                

                Vector2 prevPoint = points[points.Count - 1];                

                int pointCount = points.Count;
                numSegments += pointCount;
                for (int j = 0; j < pointCount; j++)
                {                    
                    Vector2 currentPoint = points[j];
                    
                    _shadowVertices.Add(new VertexShadow(prevPoint, currentPoint, new Vector2(0.0f, 0.0f)));
                    _shadowVertices.Add(new VertexShadow(prevPoint, currentPoint, new Vector2(1.0f, 0.0f)));
                    _shadowVertices.Add(new VertexShadow(prevPoint, currentPoint, new Vector2(0.0f, 1.0f)));
                    _shadowVertices.Add(new VertexShadow(prevPoint, currentPoint, new Vector2(1.0f, 1.0f)));

                    _shadowIndices.Add(shadowIndexOffset * 4 + 0);
                    _shadowIndices.Add(shadowIndexOffset * 4 + 1);
                    _shadowIndices.Add(shadowIndexOffset * 4 + 2);

                    _shadowIndices.Add(shadowIndexOffset * 4 + 1);
                    _shadowIndices.Add(shadowIndexOffset * 4 + 3);
                    _shadowIndices.Add(shadowIndexOffset * 4 + 2);                    

                    prevPoint = currentPoint;
                    shadowIndexOffset++;
                }

                _hullVertices.AddRange(hull.WorldPoints);

                int indexCount = hull.Indices.Count;
                for (int j = 0; j < indexCount; j++)
                    _hullIndices.Add(hull.Indices[j] + hullIndexOffset);
                hullIndexOffset += pointCount;
            }

            if (numSegments == 0)
                return null;

            Tuple<DynamicVao, DynamicVao> lightVaos;
            if (!_lightsVaos.TryGetValue(light, out lightVaos))
            {
                lightVaos = Tuple.Create(
                    DynamicVao.New(_engine.Device, VertexShadow.Layout, _shadowVertices.Count, _shadowIndices.Count, useIndices: true),
                    DynamicVao.New(_engine.Device, VertexPosition2.Layout, _hullVertices.Count, _hullIndices.Count, useIndices: true));
                _lightsVaos.Add(light, lightVaos);
            }

            lightVaos.Item1.SetVertices(_shadowVertices);
            lightVaos.Item1.SetIndices(_shadowIndices);
            lightVaos.Item2.SetVertices(_hullVertices);
            lightVaos.Item2.SetIndices(_hullIndices);

            return lightVaos;
        }

        private void BuildGraphicsResources()
        {
            _bsShadow = new BlendState
            {
                ColorWriteChannels = ColorWriteChannels.Alpha,
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One
            };            
            _bsHull = new BlendState
            {
                ColorWriteChannels = ColorWriteChannels.Alpha,
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.Zero
            };
            //_dsOccludedShadow = new DepthStencilState
            //{
            //    DepthBufferEnable = false,

            //    StencilEnable = true,
            //    StencilWriteMask = 0xff,
            //    StencilMask = 0x00,
            //    StencilFunction = CompareFunction.Always,
            //    StencilPass = StencilOperation.IncrementSaturation                           
            //};
            //_dsOccludedHull = new DepthStencilState
            //{
            //    DepthBufferEnable = false,

            //    StencilEnable = true,
            //    StencilWriteMask = 0x00,
            //    StencilMask = 0xff,
            //    StencilFunction = CompareFunction.Less,
            //    StencilPass = StencilOperation.Keep,
            //    StencilFail = StencilOperation.Keep,
            //    ReferenceStencil = 1
            //};
        }
    }        
}