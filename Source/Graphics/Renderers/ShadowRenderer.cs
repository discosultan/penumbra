using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Renderers
{
    internal class ShadowRenderer
    {                
        private readonly FastList<VertexShadow> _shadowVertices = new FastList<VertexShadow>();
        private readonly FastList<Vector2> _hullVertices = new FastList<Vector2>();
        private readonly FastList<int> _shadowIndices = new FastList<int>();
        private readonly FastList<int> _hullIndices = new FastList<int>();
        private readonly Dictionary<Light, Tuple<DynamicVao, DynamicVao>> _lightsVaos =
            new Dictionary<Light, Tuple<DynamicVao, DynamicVao>>();

        private PenumbraEngine _engine;

        private Effect _fxShadow;
        private Effect _fxDebugShadow;
        private Effect _fxHull;
        private BlendState _bsShadow;
        private BlendState _bsHull;
        private DepthStencilState _dsOccludedShadow;
        private DepthStencilState _dsOccludedHull;

        public void Load(PenumbraEngine engine)
        {            
            _engine = engine;

            _fxShadow = engine.Content.Load<Effect>("Shadow");
            _fxDebugShadow = engine.Content.Load<Effect>("ShadowDebug");
            _fxHull = engine.Content.Load<Effect>("ProjectionColor");
            
            BuildGraphicsResources();
        }        

        public void Render(Light light)
        {
            var vao = TryGetVaoForLight(light);
            if (vao == null)
                return;

            _engine.Device.RasterizerState = _engine.Rs;
            _engine.Device.DepthStencilState = light.ShadowType == ShadowType.Occluded 
                ? _dsOccludedShadow 
                : DepthStencilState.None;

            Matrix worldViewProjection = light.LocalToWorld * _engine.Camera.ViewProjection;

            if (light.CastsShadows)
            {
                // Draw shadows.
                var shadowVao = vao.Item1;
                _engine.Device.BlendState = _bsShadow;
                _fxShadow.Parameters["WorldViewProjection"].SetValue(worldViewProjection);
                _engine.Device.DrawIndexed(_fxShadow, shadowVao);

                // Draw shadows borders if debugging.
                if (_engine.Debug)
                {
                    _engine.Device.RasterizerState = _engine.RsDebug;
                    _engine.Device.BlendState = BlendState.Opaque;
                    _fxDebugShadow.Parameters["WorldViewProjection"].SetValue(worldViewProjection);
                    _fxDebugShadow.Parameters["Color"].SetValue(Color.Red.ToVector4());
                    _engine.Device.DrawIndexed(_fxDebugShadow, shadowVao);
                }
            }

            // Draw hull.            
            if (light.ShadowType == ShadowType.Occluded)
                _engine.Device.DepthStencilState = _dsOccludedHull;

            var hullVao = vao.Item2;
            _engine.Device.RasterizerState = _engine.Rs;
            _engine.Device.BlendState = _bsHull;
            _fxHull.Parameters["ViewProjection"].SetValue(_engine.Camera.ViewProjection);
            _fxHull.Parameters["Color"].SetValue(light.ShadowType == ShadowType.Solid
                ? Color.Transparent.ToVector4()
                : Color.White.ToVector4());
            _engine.Device.DrawIndexed(_fxHull, hullVao);
        }

        private Tuple<DynamicVao, DynamicVao> TryGetVaoForLight(Light light)
        {                        
            _hullVertices.Clear();
            _shadowVertices.Clear();
            _shadowIndices.Clear();
            _hullIndices.Clear();

            Vector2 result = Vector2.TransformNormal(new Vector2(light.Radius), light.WorldToLocal);
            float radius = result.X;

            int numSegments = 0;
            int shadowIndexOffset = 0;
            int hullIndexOffset = 0;
            int hullCount = _engine.Hulls.Count;
            for (int i = 0; i < hullCount; i++)
            {
                Hull hull = _engine.Hulls[i];
                if (!hull.Enabled || !hull.Valid || !light.Intersects(hull))
                    continue;
                
                Matrix t = /*hull.LocalToWorld * */light.WorldToLocal;

                //var points = hull.LocalPoints;
                var points = hull.WorldPoints;
                numSegments += points.Count;

                Vector2 prevPoint = points[points.Count - 1];
                prevPoint = Transform(t, prevPoint);
                int pointCount = points.Count;          
                for (int j = 0; j < pointCount; j++)
                {                    
                    Vector2 currentPoint = points[j];
                    currentPoint = Transform(t, currentPoint);                                                            
                    
                    _shadowVertices.Add(new VertexShadow(new Vector3(0.0f, 0.0f, radius), prevPoint, currentPoint));
                    _shadowVertices.Add(new VertexShadow(new Vector3(1.0f, 0.0f, radius), prevPoint, currentPoint));
                    _shadowVertices.Add(new VertexShadow(new Vector3(0.0f, 1.0f, radius), prevPoint, currentPoint));
                    _shadowVertices.Add(new VertexShadow(new Vector3(1.0f, 1.0f, radius), prevPoint, currentPoint));

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

        private static Vector2 Transform(Matrix m, Vector2 p)
        {
            return new Vector2(p.X * m[0] + p.Y * m[4] + m[12], p.X * m[1] + p.Y * m[5] + m[13]);
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
            _dsOccludedShadow = new DepthStencilState
            {
                DepthBufferEnable = false,

                StencilEnable = true,
                StencilWriteMask = 0xff,
                StencilMask = 0x00,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.IncrementSaturation                           
            };
            _dsOccludedHull = new DepthStencilState
            {
                DepthBufferEnable = false,

                StencilEnable = true,
                StencilWriteMask = 0x00,
                StencilMask = 0xff,
                StencilFunction = CompareFunction.Less,
                StencilPass = StencilOperation.Keep,
                StencilFail = StencilOperation.Keep,
                ReferenceStencil = 1
            };
        }
    }        
}
