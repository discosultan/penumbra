using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Penumbra.Geometry;

namespace Penumbra.Graphics
{
    internal static class GraphicsExtensions
    {
        public static void SetScissorRectangle(this GraphicsDevice device, BoundingRectangle bounds)
        {
            device.ScissorRectangle = new Rectangle(
                (int)bounds.Min.X,
                (int)bounds.Min.Y,
                (int)(bounds.Max.X - bounds.Min.X),
                (int)(bounds.Max.Y - bounds.Min.Y));
        }

        public static void DrawIndexed(this GraphicsDevice device, Effect effect, IVao vao)
        {
            device.SetVertexArrayObject(vao);
            effect.CurrentTechnique.Passes[0].Apply();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vao.VertexCount, 0, vao.IndexCount / 3);            
        }

        public static void Draw(this GraphicsDevice device, Effect effect, IVao vao)
        {
            device.SetVertexArrayObject(vao);
            effect.CurrentTechnique.Passes[0].Apply();
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, vao.VertexCount - 2);
        }
    }
}
