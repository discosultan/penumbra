using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    internal sealed class StaticVao : IVao
    {        
        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer IndexBuffer { get; }
        public int VertexCount => VertexBuffer.VertexCount;
        public int IndexCount => IndexBuffer.IndexCount;

        private StaticVao(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            IndexBuffer = indexBuffer;
            VertexBuffer = vertexBuffer;
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer?.Dispose();
        }

        public static StaticVao New<T>(
            GraphicsDevice graphicsDevice,
            IList<T> vertices,
            VertexDeclaration vertexDeclaration,
            IList<int> indices = null) where T : struct
        {
            StaticVao result;
            var vb = new VertexBuffer(graphicsDevice, vertexDeclaration, vertices.Count, BufferUsage.None);
            vb.SetData(vertices.ToArray());
            if (indices == null)
            {
                // Vertex buffer only.
                result = new StaticVao(vb, null);
            }
            else
            {
                var ib = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.None);
                ib.SetData(indices.ToArray());
                // Vertex and index buffers.
                result = new StaticVao(vb, ib);
            }
            return result;
        }    
    }    
}
