using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Penumbra.Geometry;

namespace Penumbra.Graphics
{
    internal static class GraphicsExtensions
    {
        public static void SetScissorRectangle(this GraphicsDevice device, ref BoundingRectangle bounds)
        {
            device.ScissorRectangle = new Rectangle(
                (int)bounds.Min.X,
                (int)bounds.Min.Y,
                (int)(bounds.Max.X - bounds.Min.X),
                (int)(bounds.Max.Y - bounds.Min.Y));
        }
    }
}
