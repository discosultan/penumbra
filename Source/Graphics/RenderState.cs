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

        public RenderState(DepthStencilState dsState, BlendState bState, RasterizerState rState)
        {
            DepthStencilState = dsState;
            BlendState = bState;
            RasterizerState = rState;
        }
    }
}
