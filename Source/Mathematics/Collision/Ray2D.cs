using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Collision
{
    internal struct Ray2D
    {
        public Vector2 Origin;
        public Vector2 Direction;

        public Ray2D(Vector2 origin, Vector2 direction)
            : this(ref origin, ref direction)
        {            
        }

        public Ray2D(ref Vector2 origin, ref Vector2 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Vector2 GetPoint(float distance)
        {
            return Origin + Direction * distance;
        }

        public bool Intersects(ref Ray2D ray)
        {
            Vector2 intersectionPoint;
            return Intersects(ref ray, out intersectionPoint);
        }

        public bool Intersects(ref Ray2D ray, out Vector2 intersectionPoint)
        {
            return Collision.RayIntersectsRay(ref Origin, ref Direction, ref ray.Origin, ref ray.Direction, out intersectionPoint);
        }

        public bool Intersects(ref Line2D line)
        {
            Vector2 intersectionPoint;
            return Intersects(ref line, out intersectionPoint);
        }

        public bool Intersects(ref Line2D line, out Vector2 intersectionPoint)
        {
            return Collision.RayIntersectsLine(ref Origin, ref Direction, ref line.P1, ref line.P2, out intersectionPoint);
        }
    }
}
