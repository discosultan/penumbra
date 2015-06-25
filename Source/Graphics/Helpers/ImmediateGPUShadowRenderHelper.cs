using System.Collections.Generic;
using Penumbra;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using Varus.Paradox.Light2D.Effects.Shaders;

namespace Varus.Paradox.Light2D.RenderHelpers
{
    internal class ImmediateGPUShadowRenderHelper : ComponentBase, IShadowRenderHelper
    {
        // TODO: reuse similar vaos
        private readonly Dictionary<GPUHull, Vao> _vaos = new Dictionary<GPUHull, Vao>();

        private readonly GraphicsDevice _graphicsDevice;
        private readonly LightRenderer _lightRenderer;

        private readonly HullList<GPUHull> _hulls;
        private readonly List<int> _notIntersectingHullIndices = new List<int>();
        
        public ImmediateGPUShadowRenderHelper(GraphicsDevice device, LightRenderer lightRenderer)
        {
            _graphicsDevice = device;
            _lightRenderer = lightRenderer;
            _hulls = new HullList<GPUHull>(lightRenderer.ObservableHulls, new GPUHullFactory());

            _hulls.CollectionChanged += (s, e) => BuildHullsBuffer();
            lightRenderer.ObservableLights.CollectionChanged += (s, e) => BuildHullsBuffer();
            BuildHullsBuffer();
        }

        public void DrawShadows(LightComponent light, RenderContext context, RenderProcess umbraProcess, RenderProcess penumbraProcess, RenderProcess hullProcess)
        {
            _notIntersectingHullIndices.Clear();

            // Draw penumbra.
            for (int i = 0; i < _hulls.Count; i++)
            {
                GPUHull hull = _hulls[i];
                if (light.Intersects(hull))
                {
                    _notIntersectingHullIndices.Add(i);

                    context.Parameters.Set(L2D_WorldTransformationKeys.WorldTransform, hull.Inner.WorldTransform);
                    _graphicsDevice.SetVertexArrayObject(_vaos[hull]);
                    context.Parameters.Set(L2D_ColorBaseKeys.Color, Color.Red.ToVector4());
                    foreach (RenderStep step in penumbraProcess.Steps(_lightRenderer.DebugDraw))
                    {
                        step.Apply(_graphicsDevice, context);
                        _graphicsDevice.Draw(PrimitiveType.TriangleList, hull.PenumbraVertexCount);
                    }
                }                
            }
            // Draw umbra.
            foreach (int i in _notIntersectingHullIndices)
            {
                GPUHull hull = _hulls[i];
                context.Parameters.Set(L2D_WorldTransformationKeys.WorldTransform, hull.Inner.WorldTransform);
                _graphicsDevice.SetVertexArrayObject(_vaos[hull]);
                context.Parameters.Set(L2D_ColorBaseKeys.Color, Color.Green.ToVector4());
                foreach (RenderStep step in umbraProcess.Steps(_lightRenderer.DebugDraw))
                {
                    step.Apply(_graphicsDevice, context);
                    _graphicsDevice.DrawIndexed(PrimitiveType.TriangleList, hull.UmbraIndexCount);
                }
            }
            // Draw solid.
            foreach (int i in _notIntersectingHullIndices)
            {
                GPUHull hull = _hulls[i];
                context.Parameters.Set(L2D_WorldTransformationKeys.WorldTransform, hull.Inner.WorldTransform);
                _graphicsDevice.SetVertexArrayObject(_vaos[hull]);
                context.Parameters.Set(L2D_ColorBaseKeys.Color, Color.Blue.ToVector4());
                foreach (RenderStep step in hullProcess.Steps(_lightRenderer.DebugDraw))
                {
                    step.Apply(_graphicsDevice, context);
                    _graphicsDevice.DrawIndexed(PrimitiveType.TriangleList, hull.SolidIndexCount, hull.UmbraIndexCount);
                }
            }
        }

        protected override void Destroy()
        {
            foreach (Vao vao in _vaos.Values)
            {
                vao.Dispose();
            }
            _vaos.Clear();
            base.Destroy();
        }

        private void BuildHullsBuffer()
        {
            foreach (GPUHull hull in _hulls)
            {
                if (_vaos.ContainsKey(hull)) continue;
             
                // TODO: Also remove vaos when hulls are removed.
                _vaos.Add(
                    hull,
                    //Vao.New(_graphicsDevice, hull.TransformedVertices, hull.Indices));
                    Vao.New(_graphicsDevice, hull.OriginalVertices, hull.Indices));                
            }
        }        
    }
}
