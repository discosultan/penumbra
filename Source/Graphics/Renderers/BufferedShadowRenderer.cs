using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Graphics.Builders;
using Penumbra.Mathematics;
using Penumbra.Utilities;

namespace Penumbra.Graphics.Renderers
{
    internal class BufferedShadowRenderer : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly PenumbraEngine _lightRenderer;
        private readonly HullList _hulls;
        private readonly Dictionary<Light, LightVaos> _shadowVaos = new Dictionary<Light, LightVaos>();

        private readonly PenumbraBuilder _penumbraBuilder;
        private readonly UmbraBuilder _umbraBuilder;
        private readonly SolidBuilder _solidBuilder;
        private readonly AntumbraBuilder _antumbraBuilder;

        //private readonly PointProcessingContext _currentContext = new PointProcessingContext();

        public BufferedShadowRenderer(GraphicsDevice device, PenumbraEngine lightRenderer)
        {
            _graphicsDevice = device;
            _lightRenderer = lightRenderer;
            _lightRenderer.Lights.CollectionChanged += ObservableLightsChanged;
            _hulls = new HullList(lightRenderer.Hulls);            

            _penumbraBuilder = new PenumbraBuilder();
            _umbraBuilder = new UmbraBuilder(_hulls);
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
            bool hullsDirty = _hulls.AnyDirty(HullComponentDirtyFlags.All);

            if (lightDirty || hullsDirty)
            {
                BuildVaosForLight(light, vaos);
            }

            return vaos;
        }

        private void BuildVaosForLight(Light light, LightVaos vaos)
        {
            // 1. ANY NECESSARY CLEANING OR PREPROCESSING.
            _umbraBuilder.PreProcess();
            _penumbraBuilder.PreProcess();
            _antumbraBuilder.PreProcess();            
            _solidBuilder.PreProcess();

            for (int i = 0; i < _hulls.Count; i++)
            {
                HullPart hull = _hulls[i];
                if (!hull.Enabled || !light.Intersects(hull)) continue;

                for (int j = 0; j < hull.TransformedHullVertices.Count; j++)
                {
                    HullPointContext context;
                    PopulateContextForPoint(light, hull, j, out context);

                    // 2. PROCESS GEOMETRY DATA FOR HULL POINT.
                    _umbraBuilder.ProcessHullPoint(light, hull, ref context);
                    _penumbraBuilder.ProcessHullPoint(light, hull, ref context);                    
                }

                // 3. PROCESS GEOMETRY DATA FOR HULL.  
                var hullCtx = new HullContext();
                _umbraBuilder.ProcessHull(light, hull, ref hullCtx);
                _penumbraBuilder.ProcessHull(light, hull, ref hullCtx);                
                _antumbraBuilder.ProcessHull(light, hull, ref hullCtx);
                _solidBuilder.ProcessHull(light, hull);
            }

            // 4. BUILD BUFFERS FROM PROCESSED DATA.
            _umbraBuilder.Build(light, vaos);
            _penumbraBuilder.Build(light, vaos);                     
            _antumbraBuilder.Build(light, vaos);            
            _solidBuilder.Build(light, vaos);
        }

        public void PopulateContextForPoint(Light light, HullPart hull, int i, out HullPointContext context)
        {
            Vector2 position = hull.TransformedHullVertices[i];
            context = new HullPointContext
            {
                Index = i,
                Position = position,
                Normals = hull.TransformedNormals[i],
                //IsInAnotherHull = _hulls
                //    .Where(x => x != hull)
                //    .SelectMany(x => x.TransformedHullVertices)
                //    .Any(x => x == position)
            };
            context.LightToPointDir = Vector2.Normalize(context.Position - light.Position);
            GetDotsForNormals(context.LightToPointDir, context.Normals, out context.Dot1, out context.Dot2);

            // A hull has only 1 left and 1 right side point if it is guaranteed to be CONVEX.
            context.Side = context.Dot1 >= 0 && context.Dot2 < 0
                ? Side.Left
                : context.Dot2 >= 0 && context.Dot1 < 0
                    ? Side.Right
                    : context.Dot1 >= 0 && context.Dot2 >= 0 
                        ? Side.Backward 
                        : Side.Forward;

            //_currentContext.Index = i;
            //_currentContext.Position = hull.Inner.TransformedHullVertices[i];
            //_currentContext.Normals = hull.TransformedNormals[i];
            //_currentContext.LightToPointDir = Vector2.Normalize(_currentContext.Position - light.Position);
            //GetDotsForNormals(_currentContext.LightToPointDir, _currentContext.Normals, out _currentContext.Dot1, out _currentContext.Dot2);            
        }

        private static void GetDotsForNormals(Vector2 lightToPointDir, PointNormals normals, out float dot1, out float dot2)
        {
            Vector2 normal1 = normals.Normal1;
            Vector2 normal2 = normals.Normal2;
            dot1 = Vector2.Dot(lightToPointDir, normal1);
            dot2 = Vector2.Dot(lightToPointDir, normal2);
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
