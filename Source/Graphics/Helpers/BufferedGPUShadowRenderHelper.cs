//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using SiliconStudio.Core;
//using SiliconStudio.Paradox.Effects;
//using SiliconStudio.Paradox.Engine;
//using SiliconStudio.Paradox.Graphics;
//using Varus.Paradox.Light2D.Common;
//using Varus.Paradox.Light2D.Hull;
//using Varus.Paradox.Light2D.RenderHelpers;
//using Varus.Paradox.Light2D.RenderProviders;

//namespace Varus.Paradox.Light2D
//{
//    internal class BufferedGPUShadowRenderHelper : ComponentBase, IShadowRenderHelper
//    {
//        private Vao _hullsVao;

//        private int _penumbraVertexCount;
//        private int _umbraIndexCount;
//        private int _solidIndexCount;

//        private readonly GraphicsDevice _graphicsDevice;
//        private readonly Camera _camera;
//        private readonly ObservableCollection<HullComponent> _hulls;
//        private readonly ObservableCollection<LightComponent> _lights;

//        public BufferedGPUShadowRenderHelper(GraphicsDevice device, Camera camera, ObservableCollection<HullComponent> hulls, ObservableCollection<LightComponent> lights)
//        {
//            _graphicsDevice = device;
//            _camera = camera;
//            _hulls = hulls;
//            _lights = lights;

//            _hulls.CollectionChanged += (s, e) => BuildHullsBuffer();
//            _lights.CollectionChanged += (s, e) => BuildHullsBuffer();
//            BuildHullsBuffer();
//        }

//        public void DrawShadows(Light.LightComponent light, RenderContext context, RenderProcess umbraProcess, RenderProcess penumbraProcess,
//            RenderProcess solidProcess)
//        {
//            if (_hulls.Count == 0) return;

//            _graphicsDevice.SetVertexArrayObject(_hullsVao);

//            _graphicsDevice.Draw(PrimitiveType.TriangleList, _penumbraVertexCount);
//            _graphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _umbraIndexCount, 0);
//            _graphicsDevice.DrawIndexed(PrimitiveType.TriangleList, _solidIndexCount, _umbraIndexCount);            
//        }

//        protected override void Destroy()
//        {
//            Utilities.Dispose(ref _hullsVao);
//            base.Destroy();
//        }

//        private void BuildHullsBuffer()
//        {
//            if (_hulls.Count == 0) return;

//            var vertices = new List<VertexHull>();
//            var indices = new List<int>();
//            int indexOffset = 0;
//            _penumbraVertexCount = 0;
//            _solidIndexCount = 0;
//            _umbraIndexCount = 0;

//            foreach (var hull in _hulls)
//            {
//                vertices.AddRange(hull.TransformedVertices);
//                _penumbraVertexCount += hull.PenumbraVertexCount;
//                _umbraIndexCount += hull.UmbraIndexCount;
//                _solidIndexCount += hull.SolidIndexCount;

//                // Add umbra indices.
//                for (int i = 0; i < hull.UmbraIndexCount; i++)
//                {
//                    int index = hull.Indices[i];
//                    indices.Add(indexOffset + index);
//                }
//                indexOffset += hull.OriginalVertices.Length;
//            }
//            indexOffset = 0;
//            foreach (var hull in _hulls)
//            {
//                // Add solid indices.
//                for (int i = hull.UmbraIndexCount; i < hull.UmbraIndexCount + hull.SolidIndexCount; i++)
//                {
//                    int index = hull.Indices[i];
//                    indices.Add(indexOffset + index);
//                }
//                indexOffset += hull.OriginalVertices.Length;
//            }

//            _hullsVao = Vao.New(_graphicsDevice, vertices, indices);
//        }        
//    }
//}
