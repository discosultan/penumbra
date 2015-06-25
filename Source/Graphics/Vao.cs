using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    internal sealed class Vao : IDisposable
    {
        public readonly VertexBuffer VertexBuffer;
        public readonly IndexBuffer IndexBuffer;        

        private Vao(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            IndexBuffer = indexBuffer;
            VertexBuffer = vertexBuffer;
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }

        public static Vao New<T>(
            GraphicsDevice graphicsDevice,
            IList<T> vertices,
            VertexDeclaration vertexDeclaration,
            IList<int> indices = null) where T : struct
        {
            Vao result;
            var vb = new VertexBuffer(graphicsDevice, vertexDeclaration, vertices.Count, BufferUsage.None);
            vb.SetData(vertices.ToArray());
            if (indices == null)
            {
                // Vertex buffer only.
                result = new Vao(vb, null);
            }
            else
            {
                var ib = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.None);
                ib.SetData(indices.ToArray());
                // Vertex and index buffers.
                result = new Vao(vb, ib);
            }
            return result;
        }    
    }    

    internal static class VaoExtensions
    {
        public static void SetVertexArrayObject(this GraphicsDevice device, Vao vao)
        {            
            device.SetVertexBuffer(vao.VertexBuffer);
            device.Indices = vao.IndexBuffer;
        }
    }
}
