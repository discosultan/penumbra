using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox
{
    public class PrimitiveRenderer
    {
        private readonly GraphicsDevice _device;
        private readonly Effect _effect;

        private VertexBuffer _vertexBuffer;

        public PrimitiveRenderer(GraphicsDevice device, ContentManager content)
        {
            _device = device;
            _effect = content.Load<Effect>("Primitive");
            BuildVertices();
        }

        public Matrix Transform { get; set; } = Matrix.Identity;

        private void BuildVertices()
        {
            var vertices = new[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0))
            };
            _vertexBuffer = new VertexBuffer(_device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.None);
            _vertexBuffer.SetData(vertices);
        }

        public void RenderQuad(Texture2D texture, Vector2 position, Vector2 scale)
        {
            Matrix world = Matrix.Identity;
            world *= Matrix.CreateScale(new Vector3(scale, 0));
            world *= Matrix.CreateTranslation(new Vector3(position, 0));            
                                
            _effect.Parameters["Texture"].SetValue(texture);
            _effect.Parameters["Transform"].SetValue(world * Transform);
            _effect.CurrentTechnique.Passes[0].Apply();

            _device.SetVertexBuffer(_vertexBuffer);
            _device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
    }
}
