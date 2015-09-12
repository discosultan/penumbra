using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Penumbra.Geometry;

namespace Penumbra.Graphics
{
    internal static class GraphicsExtensions
    {
        public static void SetRenderState(this GraphicsDevice device, RenderState renderState)
        {
            if (renderState.BlendState != null)
                device.BlendState = renderState.BlendState;
            if (renderState.DepthStencilState != null)
                device.DepthStencilState = renderState.DepthStencilState;
            if (renderState.RasterizerState != null)
                device.RasterizerState = renderState.RasterizerState;
        }

        public static void SetScissorRectangle(this GraphicsDevice device, BoundingRectangle bounds)
        {
            device.ScissorRectangle = new Rectangle(
                (int)bounds.Min.X,
                (int)bounds.Min.Y,
                (int)(bounds.Max.X - bounds.Min.X),
                (int)(bounds.Max.Y - bounds.Min.Y));

            //Vector2 extents = bounds.Extents;

            //device.ScissorRectangle = new Rectangle(
            //    (int)bounds.Min.X,
            //    (int)(bounds.Max.Y),
            //    (int)(extents.X * 2),
            //    (int)(extents.Y * 2));
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
