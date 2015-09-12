using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics.Renderers
{
    internal class LightRenderer
    {
        private static readonly Color DebugColor = Color.LimeGreen;

        private PenumbraEngine _engine;        

        private Effect _fxLight;
        private Effect _fxLightTexture;
        private Effect _fxLightDebug;
        private StaticVao _quadVao;
        private StaticVao _circleVao;
        private BlendState _bsLight;        

        public void Load(PenumbraEngine engine)
        {            
            _engine = engine;

            _fxLight = engine.Content.Load<Effect>("WorldProjectionLight");
            _fxLightTexture = engine.Content.Load<Effect>("WorldProjectionTexture");
            _fxLightDebug = engine.Content.Load<Effect>("WorldProjectionColor");

            BuildGraphicsResources();
        }        

        public void Render(Light light)
        {
            Effect fxLight;
            if (light.Texture == null) // Draw light using spotlight algorithm.
            {
                fxLight = _fxLight;
            }
            else // Draw light using texture.
            {
                fxLight = _fxLightTexture;
                fxLight.Parameters["TextureTransform"].SetValue(light.TextureTransform);
            }

            _engine.Device.BlendState = _bsLight;
            _engine.Device.RasterizerState = _engine.Rs;
            fxLight.Parameters["World"].SetValue(light.LocalToWorld);
            fxLight.Parameters["Color"].SetValue(light.Color.ToVector3());
            fxLight.Parameters["Intensity"].SetValue(light.IntensityFactor);
            fxLight.Parameters["ViewProjection"].SetValue(_engine.Camera.ViewProjection);
            _engine.Device.Draw(fxLight, _quadVao);

            if (_engine.Debug)
            {
                Matrix world = Matrix.Identity;
                // Scaling.
                world.M11 = light.Radius;
                world.M22 = light.Radius;
                // Translation.
                world.M41 = light.Position.X;
                world.M42 = light.Position.Y;                

                _engine.Device.BlendState = BlendState.Opaque;
                _engine.Device.RasterizerState = _engine.RsDebug;
                _fxLightDebug.Parameters["Color"].SetValue(DebugColor.ToVector4());
                _fxLightDebug.Parameters["World"].SetValue(world);
                _fxLightDebug.Parameters["ViewProjection"].SetValue(_engine.Camera.ViewProjection);
                _engine.Device.DrawIndexed(_fxLightDebug, _circleVao);
            }
        }

        private void BuildGraphicsResources()
        {            
            float d1 = (float) (2 / Math.Sqrt(2));
            float d2 = (float) (1 / Math.Sqrt(2));

            // Quad.
            VertexPosition2Texture[] quadVertices =
            {
                new VertexPosition2Texture(new Vector2(-1 - d1, +1 + d1), new Vector2(0 - d2, 0 - d2)),
                new VertexPosition2Texture(new Vector2(+1 + d1, +1 + d1), new Vector2(1 + d2, 0 - d2)),
                new VertexPosition2Texture(new Vector2(-1 - d1, -1 - d1), new Vector2(0 - d2, 1 + d2)),
                new VertexPosition2Texture(new Vector2(+1 + d1, -1 - d1), new Vector2(1 + d2, 1 + d2))
            };            
          

            _quadVao = StaticVao.New(_engine.Device, quadVertices, VertexPosition2Texture.Layout);

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

            _circleVao = StaticVao.New(_engine.Device, vertices, VertexPosition2.Layout, indices);

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
