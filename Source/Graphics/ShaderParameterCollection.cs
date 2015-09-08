using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    internal enum ShaderParameter
    {
        LightColor,
        LightIntensity,

        Color,
        ViewProjection,
        World,
        Texture,
        TextureSampler
    }

    internal class ShaderParameterCollection
    {
        private readonly Dictionary<ShaderParameter, float> _singles = new Dictionary<ShaderParameter, float>();
        private readonly Dictionary<ShaderParameter, Vector2> _vector2s = new Dictionary<ShaderParameter, Vector2>();
        private readonly Dictionary<ShaderParameter, Vector3> _vector3s = new Dictionary<ShaderParameter, Vector3>();
        private readonly Dictionary<ShaderParameter, Vector4> _vector4s = new Dictionary<ShaderParameter, Vector4>();
        private readonly Dictionary<ShaderParameter, Matrix> _matrices = new Dictionary<ShaderParameter, Matrix>();
        private readonly Dictionary<ShaderParameter, Texture2D> _textures = new Dictionary<ShaderParameter, Texture2D>();
        private readonly Dictionary<ShaderParameter, SamplerState> _textureSamplers = new Dictionary<ShaderParameter, SamplerState>();

        #region Getters

        public float GetSingle(ShaderParameter key)
        {
            return _singles[key];
        }

        public void GetVector2(ShaderParameter key, out Vector2 result)
        {
            result = _vector2s[key];
        }

        public void GetVector3(ShaderParameter key, out Vector3 result)
        {
            result = _vector3s[key];
        }

        public void GetVector4(ShaderParameter key, out Vector4 result)
        {
            result = _vector4s[key];
        }

        public void GetMatrix(ShaderParameter key, out Matrix result)
        {
            result = _matrices[key];
        }

        public Texture2D GetTexture(ShaderParameter key)
        {
            return _textures[key];
        }

        public SamplerState GetSampler(ShaderParameter key)
        {
            return _textureSamplers[key];
        }

        #endregion
        #region Setters

        public void SetSingle(ShaderParameter key, float value)
        {
            _singles[key] = value;
        }

        public void SetVector2(ShaderParameter key, Vector2 value)
        {
            _vector2s[key] = value;
        }

        public void SetVector2(ShaderParameter key, ref Vector2 value)
        {
            _vector2s[key] = value;
        }

        public void SetVector3(ShaderParameter key, Vector3 value)
        {
            _vector3s[key] = value;
        }

        public void SetVector3(ShaderParameter key, ref Vector3 value)
        {
            _vector3s[key] = value;
        }

        public void SetVector4(ShaderParameter key, Vector4 value)
        {
            _vector4s[key] = value;
        }

        public void SetVector4(ShaderParameter key, ref Vector4 value)
        {
            _vector4s[key] = value;
        }

        public void SetMatrix(ShaderParameter key, ref Matrix value)
        {
            _matrices[key] = value;
        }

        public void SetTexture(ShaderParameter key, Texture2D value)
        {
            _textures[key] = value;
        }

        public void SetSampler(ShaderParameter key, SamplerState value)
        {
            _textureSamplers[key] = value;
        }

        #endregion
    }
}
