using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Core;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Renderers
{
    internal class ShadowRenderer
    {
        private readonly GraphicsDevice _device;
        private readonly PenumbraEngine _engine;

        private readonly Effect _fxShadow;
        private readonly Effect _fxDebugShadow;
        private readonly Effect _hullFx;
        private BlendState _shadowBs;
        private BlendState _hullBs;

        private readonly FastList<VertexShadow> _shadowVertices = new FastList<VertexShadow>();
        private readonly FastList<Vector2> _hullVertices = new FastList<Vector2>();
        private readonly FastList<int> _shadowIndices = new FastList<int>();
        private readonly FastList<int> _hullIndices = new FastList<int>();

        private readonly Dictionary<Light, Tuple<DynamicVao, DynamicVao>> _lightsVaos =
            new Dictionary<Light, Tuple<DynamicVao, DynamicVao>>();

        public ShadowRenderer(GraphicsDevice device, ContentManager content, PenumbraEngine engine)
        {
            _device = device;
            _engine = engine;

            _fxShadow = content.Load<Effect>("Test");
            _fxDebugShadow = content.Load<Effect>("TestDebug");
            _hullFx = content.Load<Effect>("ProjectionColor");
            
            BuildGraphicsResources();
        }        

        public void Render(Light light)
        {
            var vao = TryGetVaoForLight(light);
            if (vao == null)
                return;

            _device.RasterizerState = _engine.Rs;
            _device.DepthStencilState = DepthStencilState.None;

            Matrix worldViewProjection = light.LocalToWorld * _engine.Camera.ViewProjection;

            // Draw shadows.
            var shadowVao = vao.Item1;            
            _device.BlendState = _shadowBs;
            _fxShadow.Parameters["WorldViewProjection"].SetValue(worldViewProjection);
            _device.DrawIndexed(_fxShadow, shadowVao);

            // Draw hull.            
            var hullVao = vao.Item2;            
            _device.BlendState = _hullBs;
            _hullFx.Parameters["ViewProjection"].SetValue(_engine.Camera.ViewProjection);
            _hullFx.Parameters["Color"].SetValue(light.ShadowType == ShadowType.Illuminated 
                ? Color.White.ToVector4() 
                : Color.Transparent.ToVector4());            
            _device.DrawIndexed(_hullFx, hullVao);

            // Draw shadows borders if debugging.
            if (_engine.Debug)
            {                
                _device.RasterizerState = _engine.RsDebug;
                _device.BlendState = BlendState.Opaque;
                _fxDebugShadow.Parameters["WorldViewProjection"].SetValue(worldViewProjection);
                _fxDebugShadow.Parameters["Color"].SetValue(Color.Red.ToVector4());
                _device.DrawIndexed(_fxDebugShadow, shadowVao);
            }
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
                    DynamicVao.New(_device, VertexShadow.Layout, _shadowVertices.Count, _shadowIndices.Count, useIndices: true),
                    DynamicVao.New(_device, VertexPosition2.Layout, _hullVertices.Count, _hullIndices.Count, useIndices: true));
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
            _shadowBs = new BlendState
            {
                ColorWriteChannels = ColorWriteChannels.Alpha,
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.One
            };            
            _hullBs = new BlendState
            {
                ColorWriteChannels = ColorWriteChannels.Alpha,
                AlphaBlendFunction = BlendFunction.Add,
                AlphaSourceBlend = Blend.One,
                AlphaDestinationBlend = Blend.Zero
            };            
        }
    }        
}
