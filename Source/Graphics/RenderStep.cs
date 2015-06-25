using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    /// <summary>
    /// Encapsulates <see cref="RenderState"/> and <see cref="Effect"/>. Comparable to Effect Framework pass.
    /// </summary>
    internal class RenderStep
    {
        private readonly RenderState _renderState;
        private readonly Effect _effect;        

        public RenderStep(RenderState renderState, Effect effect, bool isDebug = false)
        {            
            _renderState = renderState;
            _effect = effect;
            IsDebug = isDebug;
        }
        
        public bool IsDebug { get; }

        public EffectParameterCollection Parameters => _effect.Parameters;

        public void Apply(GraphicsDevice graphicsDevice)
        {            
            graphicsDevice.SetRenderState(_renderState);            
            _effect.CurrentTechnique.Passes[0].Apply();
        }

        public override string ToString()
        {
            return $"{_effect} Debug: {IsDebug}";
        }
    }
}
