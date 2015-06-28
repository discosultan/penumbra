using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct VertexPosition2Texture
    {
        public static readonly int Size = 16;

        public static readonly VertexDeclaration Layout = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

        public VertexPosition2Texture(Vector2 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;            
        }

        /// <summary>
        /// The position of the vertex.
        /// </summary>        
        public Vector2 Position;

        /// <summary>
        /// The texture coordinate of the vertex.
        /// </summary>        
        public Vector2 TexCoord;

        public override string ToString()
        {
            return $"Position:{Position} TexCoord:{TexCoord}";
        }
    }

    internal struct VertexPosition2
    {
        public static readonly VertexDeclaration Layout = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));
    }
}
