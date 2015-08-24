using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Builders;
using Penumbra.Mathematics;
using Penumbra.Utilities;
using UmbraBuilder = Penumbra.Graphics.Builders.UmbraBuilder3;
using PenumbraBuilder = Penumbra.Graphics.Builders.PenumbraBuilder2;

namespace Penumbra.Graphics.Renderers
{
    internal class BufferedShadowRenderer : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly PenumbraEngine _lightRenderer;        
        private readonly Dictionary<Light, LightVaos> _shadowVaos = new Dictionary<Light, LightVaos>();

        private readonly PenumbraBuilder _penumbraBuilder;
        private readonly UmbraBuilder _umbraBuilder;
        private readonly SolidBuilder _solidBuilder;
        private readonly AntumbraBuilder _antumbraBuilder;

        public BufferedShadowRenderer(GraphicsDevice device, PenumbraEngine lightRenderer)
        {
            _graphicsDevice = device;
            _lightRenderer = lightRenderer;
            _lightRenderer.Lights.CollectionChanged += ObservableLightsChanged;            

            _penumbraBuilder = new PenumbraBuilder();
            _umbraBuilder = new UmbraBuilder();
            _solidBuilder = new SolidBuilder();
            _antumbraBuilder = new AntumbraBuilder();
        }

        public void DrawShadows(
            Light light, 
            RenderProcess umbraProcess, 
            RenderProcess penumbraProcess, 
            RenderProcess antumbraProcess, 
            RenderProcess solidProcess)
        {
            LightVaos vaos = GetVaosForLight(light);

            // Draw penumbra.
            if (vaos.HasPenumbra)
            {                
                _graphicsDevice.SetVertexArrayObject(vaos.PenumbraVao);                
                foreach (RenderStep step in penumbraProcess.Steps(_lightRenderer.DebugDraw))
                {                                    
                    step.Apply(_lightRenderer.ShaderParameters);
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vaos.PenumbraVao.VertexBuffer.VertexCount, 0, vaos.PenumbraVao.IndexCount / 3);
                }
            }
            if (vaos.HasAntumbra)
            {
                _graphicsDevice.SetVertexArrayObject(vaos.AntumbraVao);
                foreach (RenderStep step in antumbraProcess.Steps(_lightRenderer.DebugDraw))
                {
                    step.Apply(_lightRenderer.ShaderParameters);
                    _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, vaos.AntumbraVao.VertexCount / 3);
                }
            }
            // TODO: Try enabling depth buffer for umbra. We might prevent a lot of overdraw and thus
            // TODO: potentially increase performance. Needs testing.
            // Draw umbra.
            if (vaos.HasUmbra)
            {                
                _graphicsDevice.SetVertexArrayObject(vaos.UmbraVao);                
                foreach (RenderStep step in umbraProcess.Steps(_lightRenderer.DebugDraw))
                {                                                            
                    step.Apply(_lightRenderer.ShaderParameters);
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vaos.UmbraVao.VertexBuffer.VertexCount, 0, vaos.UmbraVao.IndexCount / 3);
                }
            }
            // Draw solid.
            if (vaos.HasSolid)
            {                
                _graphicsDevice.SetVertexArrayObject(vaos.SolidVao);                
                foreach (RenderStep step in solidProcess.Steps(_lightRenderer.DebugDraw))
                {                    
                    step.Apply(_lightRenderer.ShaderParameters);
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vaos.SolidVao.VertexBuffer.VertexCount, 0, vaos.SolidVao.IndexCount / 3);
                }
            }
        }

        private LightVaos GetVaosForLight(Light light)
        {
            LightVaos vaos;
            if (!_shadowVaos.TryGetValue(light, out vaos))
            {
                vaos = new LightVaos(
                    umbra: DynamicVao.New(_graphicsDevice, VertexPosition2.Layout, useIndices: true),
                    penumbra: DynamicVao.New(_graphicsDevice, VertexPosition2Texture.Layout, useIndices: true),
                    antumbra: DynamicVao.New(_graphicsDevice, VertexPosition2Texture.Layout, useIndices: false),
                    solid: DynamicVao.New(_graphicsDevice, VertexPosition2.Layout, useIndices: true));
                _shadowVaos[light] = vaos;
            }

            // TODO: We don't need to always recreate everything
            bool lightDirty = light.AnyDirty(
                LightComponentDirtyFlags.Radius | 
                LightComponentDirtyFlags.Position | 
                LightComponentDirtyFlags.CastsShadows | 
                LightComponentDirtyFlags.ShadowType);
            bool hullsDirty = _lightRenderer.ResolvedHulls.AnyDirty(HullComponentDirtyFlags.All);

            if (lightDirty || hullsDirty)
            {
                BuildVaosForLight(light, vaos);
            }

            return vaos;
        }
        
        private readonly HullContext _hullContext = new HullContext();
        private void BuildVaosForLight(Light light, LightVaos vaos)
        {
            // 1. ANY NECESSARY CLEANING OR PREPROCESSING.
            _umbraBuilder.PreProcess();
            _penumbraBuilder.PreProcess();
            _antumbraBuilder.PreProcess();
            _solidBuilder.PreProcess();

            int hullCount = _lightRenderer.ResolvedHulls.Count;
            for (int i = 0; i < hullCount; i++)
            {
                Hull hull = _lightRenderer.ResolvedHulls[i];
                if (!hull.Enabled || !light.Intersects(hull)) continue;

                _hullContext.Clear();
                _hullContext.IsConvex = hull.TransformedPoints.IsConvex(); // TODO: Move isConvex to hull and cache it

                int pointCount = hull.TransformedPoints.Count;
                for (int j = 0; j < pointCount; j++)
                {
                    HullPointContext context;
                    GetPointContext(light, hull, j, out context);

                    _hullContext.PointContexts.Add(context);
                }

                // 3. PROCESS GEOMETRY DATA FOR HULL.                
                _umbraBuilder.ProcessHull(light, _hullContext);
                _penumbraBuilder.ProcessHull(light, hull, _hullContext);                
                _antumbraBuilder.ProcessHull(light, _hullContext);
                _solidBuilder.ProcessHull(light, hull);
            }

            // 4. BUILD BUFFERS FROM PROCESSED DATA.
            _umbraBuilder.Build(light, vaos);
            _penumbraBuilder.Build(light, vaos);                     
            _antumbraBuilder.Build(light, vaos);            
            _solidBuilder.Build(light, vaos);
        }

        public void GetPointContext(Light light, Hull hull, int i, out HullPointContext context)
        {
            Vector2 position = hull.TransformedPoints[i];
            context = new HullPointContext
            {
                Index = i,
                Point = position,
                Normals = hull.TransformedNormals[i]
            };
            
            Vector2.Subtract(ref context.Point, ref light._position, out context.LightToPointDir);
            Vector2.Normalize(ref context.LightToPointDir, out context.LightToPointDir);

            //context.LightToPointDir = Vector2.Normalize(context.Point - light.Position);
            GetDotsForNormals(ref context.LightToPointDir, ref context.Normals, out context.Dot1, out context.Dot2);

            GetUmbraVectors(light, ref position, ref context.LightToPointDir, +1f, out context.LightRight, out context.LightRightToPointDir);
            GetUmbraVectors(light, ref position, ref context.LightToPointDir, -1f, out context.LightLeft, out context.LightLeftToPointDir);

            GetDotsForNormals(ref context.LightRightToPointDir, ref context.Normals, out context.RightDot1, out context.RightDot2);
            GetDotsForNormals(ref context.LightLeftToPointDir, ref context.Normals, out context.LeftDot1, out context.LeftDot2);

            context.Side = GetSide(context.Dot1, context.Dot2);
            context.LeftSide = GetSide(context.LeftDot1, context.LeftDot2);
            context.RightSide = GetSide(context.RightDot1, context.RightDot2);           
        }

        private static Side GetSide(float dot1, float dot2)
        {
            return dot1 >= 0 && dot2 < 0
                    ? Side.Left
                    : dot2 >= 0 && dot1 < 0
                        ? Side.Right
                        : dot1 >= 0 && dot2 >= 0
                            ? Side.Backward
                            : Side.Forward;
        }

        private static void GetDotsForNormals(ref Vector2 lightToPointDir, ref PointNormals normals, out float dot1, out float dot2)
        {
            Vector2.Dot(ref lightToPointDir, ref normals.Normal1, out dot1);
            Vector2.Dot(ref lightToPointDir, ref normals.Normal2, out dot2);
        }

        // TODO: impr perf
        private void GetUmbraVectors(Light light, ref Vector2 position, ref Vector2 lightToPointDir, float project, out Vector2 lightSide,
            out Vector2 lightSideToCurrentDir)
        {
            var lightToCurrent90CWDir = VectorUtil.Rotate90CW(lightToPointDir);

            lightSide = light.Position + lightToCurrent90CWDir * light.Radius * project;
            lightSideToCurrentDir = Vector2.Normalize(position - lightSide);
        }

        private void ObservableLightsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (Light light in e.OldItems)
                        _shadowVaos.Remove(light);
                    Logger.Write("Light removed from vaos");
                    break;
            }
        }

        public void Dispose()
        {
            foreach (LightVaos vaos in _shadowVaos.Values)
            {
                vaos.Dispose();
            }
            _shadowVaos.Clear();
        }
    }
}
