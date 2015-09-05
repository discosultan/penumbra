using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Core;

namespace Penumbra.Graphics.Renderers
{
    internal sealed class PrimitiveRenderer : IDisposable
    {
        private static readonly object Lock = new object();
        private static int _refCounter;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly PenumbraEngine _lightRenderer;

        private static Vao _fullscreenQuadVao;
        private static Vao _quadVao;
        private static Vao _circleVao;

        public PrimitiveRenderer(GraphicsDevice device, PenumbraEngine lightRenderer)
        {            
            _graphicsDevice = device;
            _lightRenderer = lightRenderer;
            lock (Lock)
            {
                if (_refCounter <= 0)
                {
                    BuildQuadBuffers();
                    BuildCircleBuffer();       
                }
                _refCounter++;
            }
        }

        public void DrawCircle(RenderProcess process, Vector2 position, float radius)
        {
            Matrix transform = Matrix.Identity;
            // Scaling.
            transform.M11 = radius;
            transform.M22 = radius;
            // Translation.
            transform.M41 = position.X;
            transform.M42 = position.Y;

            _lightRenderer.ShaderParameters.SetMatrix(ShaderParameter.WorldTransform, ref transform);            

            _graphicsDevice.SetVertexArrayObject(_circleVao);
            foreach (RenderStep step in process.Steps(_lightRenderer.DebugDraw))
            {                
                step.Apply(_lightRenderer.ShaderParameters);
                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _circleVao.VertexBuffer.VertexCount, 0, _circleVao.IndexBuffer.IndexCount / 3);
            }
        }

        public void DrawQuad(RenderProcess process, Vector2 position, float size)
        {            
            float halfSize = size / 2;
            Matrix transform = Matrix.Identity;
            // Scaling.
            transform.M11 = halfSize;
            transform.M22 = halfSize;
            // Translation.
            transform.M41 = position.X;
            transform.M42 = position.Y;

            _lightRenderer.ShaderParameters.SetMatrix(ShaderParameter.WorldTransform, ref transform);            

            _graphicsDevice.SetVertexArrayObject(_quadVao);
            foreach (RenderStep step in process.Steps(_lightRenderer.DebugDraw))
            {                
                step.Apply(_lightRenderer.ShaderParameters);
                _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

        public void DrawFullscreenQuad(RenderProcess process, Texture2D texture)
        {            
            _lightRenderer.ShaderParameters.SetTexture(ShaderParameter.Texture, texture);
            _lightRenderer.ShaderParameters.SetSampler(ShaderParameter.TextureSampler, SamplerState.LinearClamp);            
            DrawFullscreenQuad(process);
        }

        public void DrawFullscreenQuad(RenderProcess process)
        {
            _graphicsDevice.SetVertexArrayObject(_fullscreenQuadVao);
            foreach (RenderStep step in process.Steps(_lightRenderer.DebugDraw))
            {
                step.Apply(_lightRenderer.ShaderParameters);
                _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            }
        }
         
        public void Dispose()
        {
            lock (Lock)
            {
                _refCounter--;
                if (_refCounter <= 0)
                {
                    _quadVao?.Dispose();
                    _fullscreenQuadVao?.Dispose();
                    _circleVao?.Dispose();                    
                }
            }            
        }      

        private void BuildQuadBuffers()
        {
            VertexPosition2Texture[] fullscreenQuadVertices =
            {
                new VertexPosition2Texture(new Vector2(-1f, 1f),  new Vector2(0.0f, 0.0f)),
                new VertexPosition2Texture(new Vector2(3f, 1f), new Vector2(2f, 0.0f)),
                new VertexPosition2Texture(new Vector2(-1f, -3f), new Vector2(0.0f, 2f))
            };            

            VertexPosition2Texture[] quadVertices =
            {                
                new VertexPosition2Texture(new Vector2(-1, +1), new Vector2(0, 0)),
                new VertexPosition2Texture(new Vector2(+1, +1), new Vector2(1, 0)),
                new VertexPosition2Texture(new Vector2(-1, -1), new Vector2(0, 1)),
                new VertexPosition2Texture(new Vector2(+1, -1), new Vector2(1, 1))
            };

            _fullscreenQuadVao = Vao.New(_graphicsDevice, fullscreenQuadVertices, VertexPosition2Texture.Layout);
            _quadVao = Vao.New(_graphicsDevice, quadVertices, VertexPosition2Texture.Layout);
        }

        private void BuildCircleBuffer()
        {
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

            _circleVao = Vao.New(_graphicsDevice, vertices, VertexPosition2.Layout, indices);
        }
    }
}
