using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    internal sealed class StaticVao : Vao
    {
        private StaticVao(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, VertexDeclaration vertexDeclaration, PrimitiveType primitiveTopology)
            : base(vertexDeclaration, primitiveTopology)
        {
            IndexBuffer = indexBuffer;
            VertexBuffer = vertexBuffer;
            CalculatePrimitiveCount();
        }

        public static StaticVao New<T>(
            GraphicsDevice graphicsDevice,
            IList<T> vertices,
            VertexDeclaration vertexDeclaration,
            PrimitiveType primitiveTopology,
            IList<int> indices = null) where T : struct
        {
            StaticVao result;
            var vb = new VertexBuffer(graphicsDevice, vertexDeclaration, vertices.Count, BufferUsage.None);
            vb.SetData(vertices.ToArray());
            if (indices == null)
            {
                // Vertex buffer only.
                result = new StaticVao(vb, null, vertexDeclaration, primitiveTopology);
            }
            else
            {
                var ib = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.None);
                ib.SetData(indices.ToArray());
                // Vertex and index buffers.
                result = new StaticVao(vb, ib, vertexDeclaration, primitiveTopology);
            }
            return result;
        }
    }
}
