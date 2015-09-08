using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Penumbra.Graphics
{
    internal static class VertexPosition2
    {
        public static readonly int Size = 8;

        public static readonly VertexDeclaration Layout = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));

        // Missing implementation. Use Vector2 instead!
    }

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
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct VertexShadow
    {
        public const int Size = 28;

        public Vector3 OccluderCoordRadius;
        public Vector2 SegmentA;
        public Vector2 SegmentB;

        public VertexShadow(Vector3 occ, Vector2 segA, Vector2 segB)
        {
            OccluderCoordRadius = occ;
            SegmentA = segA;
            SegmentB = segB;
        }

        public static readonly VertexDeclaration Layout = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1));        
    }
}
