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
            // There seem to be 1 pixel offset errors converting rectangle from world -> ndc -> screen.
            // Current fix is to add a 1 px margin to the screen rectangle.
            const int pixelErrorFix = 1;
            device.ScissorRectangle = new Rectangle(
                (int) bounds.Min.X + pixelErrorFix,
                (int) bounds.Min.Y + pixelErrorFix,
                (int) (bounds.Max.X - bounds.Min.X) - pixelErrorFix * 2,
                (int) (bounds.Max.Y - bounds.Min.Y) - pixelErrorFix * 2);
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
