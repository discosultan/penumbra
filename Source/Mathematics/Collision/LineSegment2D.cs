using System.Globalization;
using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Collision
{
    internal struct LineSegment2D
    {
        public Vector2 P1;
        public Vector2 P2;

        public LineSegment2D(Vector2 p1, Vector2 p2)
            : this(ref p1, ref p2)
        {            
        }

        public LineSegment2D(ref Vector2 p1, ref Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public bool Intersects(ref Line2D line)
        {
            Vector2 intersectionPoint;
            return Intersects(ref line, out intersectionPoint);
        }

        public bool Intersects(ref Line2D line, out Vector2 intersectionPoint)
        {
            return Collision.LineIntersect(ref P1, ref P2, ref line.P1, ref line.P2, true, false, out intersectionPoint);
        }

        public bool Intersects(ref LineSegment2D lineSegment)
        {
            Vector2 intersectionPoint;
            return Intersects(ref lineSegment, out intersectionPoint);
        }

        public bool Intersects(ref LineSegment2D lineSegment, out Vector2 intersectionPoint)
        {
            return Collision.LineIntersect(ref P1, ref P2, ref lineSegment.P1, ref lineSegment.P2, true, true, out intersectionPoint);
        }

        public bool Intersects(ref Ray2D ray)
        {
            Vector2 intersectionPoint;
            return Intersects(ref ray, out intersectionPoint);
        }

        public bool Intersects(ref Ray2D ray, out Vector2 intersectionPoint)
        {
            float distance;
            bool intersects = Collision.RayIntersectsLineSegment(ref ray.Origin, ref ray.Direction, ref P1, ref P2, out distance);
            intersectionPoint = ray.GetPoint(distance);
            return intersects;
        }

        //public bool Intersects(Polygon polygon, out Vector2 intersectionPoint)
        //{
            
        //}

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "P1:{0} P2:{1}", P1, P2);
        }
    }
}
