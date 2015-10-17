using System;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    internal interface IVao : IDisposable
    {
        int VertexCount { get; }
        int IndexCount { get; }
        VertexBuffer VertexBuffer { get; }
        IndexBuffer IndexBuffer { get; }
    }

    internal static class VaoExtensions
    {
        public static void SetVertexArrayObject(this GraphicsDevice device, IVao vao)
        {
            device.SetVertexBuffer(vao.VertexBuffer);
            if (vao.IndexBuffer != null)
                device.Indices = vao.IndexBuffer;
        }
    }
}
