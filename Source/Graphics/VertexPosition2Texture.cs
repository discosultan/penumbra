using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Mathematics;

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

        // TODO: use out;
        public static VertexPosition2Texture InterpolateTexCoord(ref VertexPosition2Texture v1,
            ref VertexPosition2Texture v2, ref VertexPosition2Texture v3, ref Vector2 p)
        {            
            Vector3 barycentricCoords;
            VectorUtil.Barycentric(
                ref p,
                ref v1.Position,
                ref v2.Position,
                ref v3.Position,
                out barycentricCoords);
            Vector2 interpolatedTexCoord =
                v1.TexCoord * barycentricCoords.X +
                v2.TexCoord * barycentricCoords.Y +
                v3.TexCoord * barycentricCoords.Z;

            return new VertexPosition2Texture(
                p,
                interpolatedTexCoord);
        }
    }

    internal static class VertexPosition2
    {
        public static readonly VertexDeclaration Layout = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0));

        // Missing implementation. Use Vector2 instead!
    }
}
