using System.Globalization;
using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Collision
{
    internal enum LineIntersectionResult
    {
        CollinearNonIntersecting,
        Collinear,
        Parallel,
        NonIntersecting,
        Intersecting
    }

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

        //public bool Intersects(ref LineSegment2D lineSegment)
        //{
        //    Vector2 intersectionPoint;
        //    return Intersects(ref lineSegment, out intersectionPoint);
        //}

        //public bool Intersects(ref LineSegment2D lineSegment, out Vector2 intersectionPoint)
        //{
        //    //return Collision.LineIntersect2(ref P1, ref P2, ref lineSegment.P1, ref lineSegment.P2, true, true, out intersectionPoint);
        //    return Collision.LineIntersect2(ref P1, ref P2, ref lineSegment.P1, ref lineSegment.P2, out intersectionPoint);
        //}

        // ref: http://stackoverflow.com/a/565282/1466456
        //public LineIntersectionResult Intersects(ref LineSegment2D lineSegment, out Vector2 intersectionPoint)
        //{
        //    Vector2 p = P1;
        //    Vector2 r = P2 - P1;
        //    Vector2 q = lineSegment.P1;
        //    Vector2 s = lineSegment.P2 - lineSegment.P1;

        //    float rsX = VectorUtil.Cross(r, s);
        //    float pqrX = VectorUtil.Cross(q - p, r);

        //    if (Calc.NearZero(rsX) && Calc.NearZero(pqrX))
        //    {
        //        float rrDot = Vector2.Dot(r, r);
        //        float t0 = Vector2.Dot(q - p, r)/rrDot;
        //        //float t1 = Vector2.Dot(q + s - p, r)/rrDot;
        //        float srDot = Vector2.Dot(s, r);
        //        float t1 = t0 + srDot/rrDot;

        //        float t1mt0 = t1 - t0;

        //        if (srDot >= 0 && t1mt0 <= 1 && t1mt0 >= 0)
        //        {
        //            intersectionPoint = 
        //            return LineIntersectionResult.Collinear;
        //        }
        //    }

        //        float u = pqrX/rsX;
                                
        //}

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
            return $"P1:{P1} P2:{P2}";
        }
    }
}
