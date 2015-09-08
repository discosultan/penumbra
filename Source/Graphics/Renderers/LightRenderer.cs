using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Core;

namespace Penumbra.Graphics.Renderers
{
    internal class LightRenderer
    {
        private readonly GraphicsDevice _device;
        private readonly PenumbraEngine _engine;

        private StaticVao _quadVao;
        private StaticVao _circleVao;
        private BlendState _bsLight;

        private readonly Effect _fxLight;
        private readonly Effect _fxLightDebug; 

        public LightRenderer(GraphicsDevice device, ContentManager content, PenumbraEngine engine)
        {
            _device = device;
            _engine = engine;
            _fxLight = content.Load<Effect>("WorldProjectionLight");
            _fxLightDebug = content.Load<Effect>("WorldProjectionColor");
            BuildGraphicsResources();
        }        

        public void Render(Light light)
        {
            _device.BlendState = _bsLight;
            _device.RasterizerState = _engine.Rs;
            _fxLight.Parameters["World"].SetValue(light.LocalToWorld);
            _fxLight.Parameters["Color"].SetValue(light.Color.ToVector3());
            _fxLight.Parameters["Intensity"].SetValue(light.IntensityFactor);
            _fxLight.Parameters["ViewProjection"].SetValue(_engine.Camera.ViewProjection);
            _device.Draw(_fxLight, _quadVao);

            if (_engine.Debug)
            {
                Matrix world = Matrix.Identity;
                // Scaling.
                world.M11 = light.Radius;
                world.M22 = light.Radius;
                // Translation.
                world.M41 = light.Position.X;
                world.M42 = light.Position.Y;                

                _device.BlendState = BlendState.Opaque;
                _device.RasterizerState = _engine.RsDebug;
                _fxLightDebug.Parameters["World"].SetValue(world);
                _fxLightDebug.Parameters["ViewProjection"].SetValue(_engine.Camera.ViewProjection);
                _device.DrawIndexed(_fxLightDebug, _circleVao);
            }
        }

        private void BuildGraphicsResources()
        {
            // Quad.

            VertexPosition2Texture[] quadVertices =
            {
                new VertexPosition2Texture(new Vector2(-1, +1), new Vector2(0, 0)),
                new VertexPosition2Texture(new Vector2(+1, +1), new Vector2(1, 0)),
                new VertexPosition2Texture(new Vector2(-1, -1), new Vector2(0, 1)),
                new VertexPosition2Texture(new Vector2(+1, -1), new Vector2(1, 1))
            };
            
            _quadVao = StaticVao.New(_device, quadVertices, VertexPosition2Texture.Layout);

            // Circle.

            const short circlePoints = 12;
            const float radius = 1f;
            const float rotationIncrement = MathHelper.TwoPi / circlePoints;

            var vertices = new Vector2[circlePoints + 1];
            var indices = new int[circlePoints * 3];

            var center = new Vector2(0, 0);
            vertices[0] = center;
            for (int i = 1; i <= circlePoints; i++)
            {
                var angle = rotationIncrement * i;
                vertices[i] = new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);

                int indexStart = (i - 1) * 3;
                indices[indexStart++] = 0;
                indices[indexStart++] = i;
                indices[indexStart] = i + 1;
            }
            indices[indices.Length - 1] = 1;

            _circleVao = StaticVao.New(_device, vertices, VertexPosition2.Layout, indices);

            // Render states.
            _bsLight = new BlendState
            {
                ColorBlendFunction = BlendFunction.Add,
                ColorSourceBlend = Blend.DestinationAlpha,
                ColorDestinationBlend = Blend.One,
                ColorWriteChannels = ColorWriteChannels.All
            };
        }
    }
}
