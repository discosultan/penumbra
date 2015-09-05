using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Core;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Renderers
{
    internal class DynamicShadowRenderer
    {
        private readonly GraphicsDevice _device;
        private readonly PenumbraEngine _engine;

        private readonly Effect _shadowFx;
        private readonly Effect _hullFx;
        private RasterizerState _cullCCWRs;
        private RasterizerState _cullCWRs;
        private BlendState _shadowBs;
        private BlendState _hullBs;

        private readonly FastList<VertexTest> _shadowVertices = new FastList<VertexTest>();
        private readonly FastList<Vector2> _hullVertices = new FastList<Vector2>();
        private readonly FastList<int> _shadowIndices = new FastList<int>();
        private readonly FastList<int> _hullIndices = new FastList<int>();

        private readonly Dictionary<Light, Tuple<DynamicVao, DynamicVao>> _lightsVaos =
            new Dictionary<Light, Tuple<DynamicVao, DynamicVao>>();

        public DynamicShadowRenderer(GraphicsDevice device, ContentManager content, PenumbraEngine engine)
        {
            _device = device;
            _engine = engine;

            _shadowFx = content.Load<Effect>("Test");
            _hullFx = content.Load<Effect>("ProjectionColor");

            BuildRenderStates();
        }        

        public void DrawShadows(Light light)
        {
            var vao = TryGetVaoForLight(light);
            if (vao == null)
                return;

            _device.RasterizerState = _engine.Camera.InvertedY ? _cullCWRs : _cullCCWRs;
            _device.DepthStencilState = DepthStencilState.None;

            // Draw shadows.
            var shadowVao = vao.Item1;
            _device.SetVertexArrayObject(shadowVao);            
            _device.BlendState = _shadowBs;
            _shadowFx.Parameters["WVP"].SetValue(light.LocalToWorld * _engine.Camera.WorldViewProjection);
            _shadowFx.CurrentTechnique.Passes[0].Apply();
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shadowVao.VertexCount, 0, shadowVao.IndexCount/3);

            // Draw hull.
            var hullVao = vao.Item2;
            _device.SetVertexArrayObject(hullVao);
            _device.BlendState = _hullBs;
            _hullFx.Parameters["ProjectionTransform"].SetValue(_engine.Camera.WorldViewProjection);
            _hullFx.Parameters["Color"].SetValue(light.ShadowType == ShadowType.Illuminated 
                ? Color.White.ToVector4() 
                : Color.Transparent.ToVector4());
            _hullFx.CurrentTechnique.Passes[0].Apply();
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, hullVao.VertexCount, 0, hullVao.IndexCount/3);
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
            foreach (var hull in _engine.ResolvedHulls.ResolvedHulls)
            {
                if (!hull.Enabled || !hull.Valid || !light.Intersects(hull))
                    continue;
                
                Matrix t = hull.LocalToWorld * light.WorldToLocal;

                //var points = hull.TransformedPoints;
                var points = hull.LocalPoints;
                numSegments += points.Count;

                Vector2 prevPoint = points[points.Count - 1];
                prevPoint = Transform(t, prevPoint);
                int pointCount = points.Count;          
                for (int i = 0; i < pointCount; i++)
                {                    
                    Vector2 currentPoint = points[i];
                    currentPoint = Transform(t, currentPoint);                                                            
                    
                    _shadowVertices.Add(new VertexTest(new Vector3(0.0f, 0.0f, radius), prevPoint, currentPoint));
                    _shadowVertices.Add(new VertexTest(new Vector3(1.0f, 0.0f, radius), prevPoint, currentPoint));
                    _shadowVertices.Add(new VertexTest(new Vector3(0.0f, 1.0f, radius), prevPoint, currentPoint));
                    _shadowVertices.Add(new VertexTest(new Vector3(1.0f, 1.0f, radius), prevPoint, currentPoint));

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
                for (int i = 0; i < indexCount; i++)
                {
                    _hullIndices.Add(hull.Indices[i] + hullIndexOffset);
                }
                hullIndexOffset += hull.WorldPoints.Count;
            }

            if (numSegments == 0)
                return null;

            Tuple<DynamicVao, DynamicVao> lightVaos;
            if (!_lightsVaos.TryGetValue(light, out lightVaos))
            {
                lightVaos = Tuple.Create(
                    DynamicVao.New(_device, VertexTest.Layout, _shadowVertices.Count, _shadowIndices.Count, useIndices: true),
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

        private void BuildRenderStates()
        {
            _cullCCWRs = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                ScissorTestEnable = true
            };
            _cullCWRs = new RasterizerState
            {
                CullMode = CullMode.CullClockwiseFace,
                ScissorTestEnable = true
            };
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

    [StructLayout(LayoutKind.Sequential)]
    internal struct VertexTest
    {
        public Vector3 OccluderCoordRadius;
        public Vector2 SegmentA;
        public Vector2 SegmentB;

        public VertexTest(Vector3 occ, Vector2 segA, Vector2 segB)
        {
            OccluderCoordRadius = occ;
            SegmentA = segA;
            SegmentB = segB;
        }

        public static readonly VertexDeclaration Layout = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1));

        public const int Size = 28;
    }
}
