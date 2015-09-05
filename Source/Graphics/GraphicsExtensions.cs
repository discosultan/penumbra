using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Penumbra.Geometry;

namespace Penumbra.Graphics
{
    internal static class GraphicsExtensions
    {
        public static void SetRenderState(this GraphicsDevice graphicsDevice, RenderState renderState)
        {
            if (renderState.BlendState != null)
                graphicsDevice.BlendState = renderState.BlendState;
            if (renderState.DepthStencilState != null)
                graphicsDevice.DepthStencilState = renderState.DepthStencilState;
            if (renderState.RasterizerState != null)
                graphicsDevice.RasterizerState = renderState.RasterizerState;
        }

        public static void SetScissorRectangle(this GraphicsDevice graphicsDevice, BoundingRectangle bounds)
        {
            graphicsDevice.ScissorRectangle = new Rectangle(
                (int) bounds.Min.X,
                (int) bounds.Min.Y,
                (int) (bounds.Max.X - bounds.Min.X),
                (int) (bounds.Max.Y - bounds.Min.Y));
        }
    }
}
