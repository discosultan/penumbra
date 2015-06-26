using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    /// <summary>
    /// Encapsulates <see cref="RenderState"/> and <see cref="Effect"/>. Comparable to Effect Framework pass.
    /// </summary>
    internal class RenderStep
    {
        private readonly GraphicsDevice _device;
        private readonly RenderState _renderState;
        private readonly Effect _effect;
        private readonly ShaderParameter[] _requiredParameters;  

        public RenderStep(
            GraphicsDevice device, 
            RenderState renderState, 
            Effect effect, 
            ShaderParameter[] parameters = null, 
            bool isDebug = false)
        {
            _requiredParameters = parameters ?? new ShaderParameter[0];
            _device = device;
            _renderState = renderState;
            _effect = effect;
            IsDebug = isDebug;
        }
        
        public bool IsDebug { get; }        

        public void Apply(ShaderParameterCollection parameters)
        {
            MapParametersToEffect(parameters);
            _device.SetRenderState(_renderState);            
            _effect.CurrentTechnique.Passes[0].Apply();
        }

        private void MapParametersToEffect(ShaderParameterCollection parameters)
        {
            foreach (ShaderParameter reqParam in _requiredParameters)
            {
                switch (reqParam)
                {
                    case ShaderParameter.LightColor:
                    {
                        Vector3 result;
                        parameters.GetVector3(reqParam, out result);
                        _effect.Parameters[nameof(ShaderParameter.LightColor)].SetValue(result);
                        break;
                    }
                    case ShaderParameter.LightIntensity:
                    {                        
                        _effect.Parameters[nameof(ShaderParameter.LightIntensity)].SetValue(parameters.GetSingle(reqParam));
                        break;
                    }
                    case ShaderParameter.LightPosition:
                    {
                        Vector2 result;
                        parameters.GetVector2(reqParam, out result);
                        _effect.Parameters[nameof(ShaderParameter.LightPosition)].SetValue(result);
                        break;
                    }
                    case ShaderParameter.LightRadius:
                    {
                        _effect.Parameters[nameof(ShaderParameter.LightRadius)].SetValue(parameters.GetSingle(reqParam));
                        break;
                    }
                    case ShaderParameter.LightRange:
                    {
                        _effect.Parameters[nameof(ShaderParameter.LightRange)].SetValue(parameters.GetSingle(reqParam));
                        break;
                    }
                    case ShaderParameter.Color:
                    {
                        Vector4 result;
                        parameters.GetVector4(reqParam, out result);
                        _effect.Parameters[nameof(ShaderParameter.Color)].SetValue(result);
                        break;
                    }
                    case ShaderParameter.ProjectionTransform:
                    {
                        Matrix result;
                        parameters.GetMatrix(reqParam, out result);
                        _effect.Parameters[nameof(ShaderParameter.ProjectionTransform)].SetValue(result);
                        break;
                    }
                    case ShaderParameter.WorldTransform:
                    {
                        Matrix result;
                        parameters.GetMatrix(reqParam, out result);
                        _effect.Parameters[nameof(ShaderParameter.WorldTransform)].SetValue(result);
                        break;
                    }
                    case ShaderParameter.Texture:
                    {
                        _effect.Parameters[nameof(ShaderParameter.Texture)].SetValue(
                            parameters.GetTexture(reqParam));
                        break;
                    }
                    case ShaderParameter.TextureSampler:
                    {
                        _device.SamplerStates[0] = parameters.GetSampler(reqParam);
                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{_effect} Debug: {IsDebug}";
        }
    }
}
