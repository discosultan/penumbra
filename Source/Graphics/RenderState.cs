using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    /// <summary>
    /// Encapsulates <see cref="DepthStencilState"/>, <see cref="BlendState"/> and <see cref="RasterizerState"/>.
    /// </summary>
    internal class RenderState
    {
        public DepthStencilState DepthStencilState { get; }
        public BlendState BlendState { get; }
        public RasterizerState RasterizerState { get; }
        public int StencilReference { get; }

        public RenderState(DepthStencilState dsState, BlendState bState, RasterizerState rState, int stencilReference = 0)
        {
            DepthStencilState = dsState;
            BlendState = bState;
            RasterizerState = rState;
            StencilReference = stencilReference;
        }
    }

    internal static class RenderStateExtensions
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
    }
}
